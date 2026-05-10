using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickUp : MonoBehaviour
{
    private Transform LeftHand;
    private Transform RightHand;
    [SerializeField] private float placementDistance = 3f;
    [SerializeField] private LayerMask placementMask = ~0;
    [SerializeField] private Material hoverMaterial;

    private Transform heldObject;
    private GameObject hoverObject;

    private void Awake()
    {
        LeftHand = transform.GetChild(2);
        RightHand = transform.GetChild(3);
    }

    private void Start()
    {
        // Removed manual subscriptions to prevent double-calling with PlayerInput messages
    }

    private void OnDestroy()
    {
    }

    private void Update()
{
        if (heldObject != null)
        {
            UpdateHover();
        }
        else
        {
            DestroyHover();
        }
    }

    private int lastActionFrame = -1;
    private int originalLayer;

    public void OnTapInteract()
    {
        if (Time.frameCount == lastActionFrame) return;
        lastActionFrame = Time.frameCount;

        if (heldObject != null)
        {
            TryPlaceHeldObject();
            return;
        }

        if (InteractionManager.Ins == null) return;
        var hoveredObject = InteractionManager.Ins.GetHoveredObject();
        if (hoveredObject == null) return;

        if (TryGetInteractable(hoveredObject, out IInteractable interactable))
        {
            Debug.Log("Interacting with: " + hoveredObject.name);
            if (AudioManager.Ins != null)
                AudioManager.Ins.PlayBubbleInteractSound();
            interactable.Interact(this);
            return;
        }

        TryPickUpObject(hoveredObject);
    }

    public void OnHoldInteract()
    {
        if (heldObject == null) return;
        TryPlaceHeldObject();
    }

    public void TryPickUpObject(GameObject target)
    {
        if (target == null) return;
        if (heldObject != null) return;

        Debug.Log($"[PlayerPickUp] Picking up: {target.name}");

        originalLayer = target.layer;
        SetLayerRecursively(target.transform, LayerMask.NameToLayer("Ignore Raycast"));

        Rigidbody body = target.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.isKinematic = true;
        }

        target.transform.SetParent(RightHand);
        target.transform.localPosition = Vector3.zero;
        target.transform.localRotation = Quaternion.identity;
        heldObject = target.transform;
        CreateHover();
    }

    public System.Action<GameObject> OnObjectPlaced;

    public GameObject GetHeldObject()
    {
        return heldObject != null ? heldObject.gameObject : null;
    }

    private void TryPlaceHeldObject()
    {
        if (heldObject == null) return;

        Debug.Log($"[PlayerPickUp] Placing: {heldObject.name}");

        Vector3 placePosition;
        Quaternion placeRotation;
        bool snapped = false;
        
        // Check for snap target first
        if (Experiment1Manager.Instance != null && Experiment1Manager.Instance.TryGetSnapTarget(heldObject.gameObject, out Vector3 snapPos, out Vector3 snapRot))
        {
            placePosition = snapPos;
            placeRotation = Quaternion.Euler(snapRot);
            snapped = true;
            Debug.Log($"[PlayerPickUp] Snapping {heldObject.name} to target.");
        }
        else
        {
            placeRotation = heldObject.rotation;
            GetPlacementPose(out placePosition, out placeRotation);
        }

        GameObject obj = heldObject.gameObject;

        heldObject.SetParent(null);
        heldObject.position = placePosition;
        heldObject.rotation = placeRotation;

        SetLayerRecursively(heldObject, originalLayer);

        Rigidbody body = heldObject.GetComponent<Rigidbody>();
        if (body != null)
        {
            // If snapped, keep kinematic to stay in place. Otherwise, enable physics.
            body.isKinematic = snapped;
            if (snapped) body.useGravity = false;
        }

        heldObject = null;
        DestroyHover();

        OnObjectPlaced?.Invoke(obj);
    }

    private void UpdateHover()
    {
        if (hoverObject == null) CreateHover();
        if (hoverObject == null) return;

        Vector3 placePosition;
        Quaternion placeRotation;
        GetPlacementPose(out placePosition, out placeRotation);
        hoverObject.transform.SetPositionAndRotation(placePosition, placeRotation);
    }

    private void GetPlacementPose(out Vector3 position, out Quaternion rotation)
    {
        rotation = heldObject != null ? heldObject.rotation : Quaternion.identity;
        Camera cam = Camera.main;
        if (cam == null || heldObject == null)
        {
            position = transform.position;
            return;
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, placementDistance, placementMask, QueryTriggerInteraction.Ignore))
        {
            Bounds bounds = GetObjectBounds(heldObject.gameObject);
            Vector3 extents = bounds.extents;
            Vector3 absNormal = new Vector3(Mathf.Abs(hit.normal.x), Mathf.Abs(hit.normal.y), Mathf.Abs(hit.normal.z));
            float offset = extents.x * absNormal.x + extents.y * absNormal.y + extents.z * absNormal.z;
            position = hit.point + hit.normal * offset;
            rotation = heldObject.rotation;
            return;
        }

        position = cam.transform.position + cam.transform.forward * placementDistance;
    }

    private Bounds GetObjectBounds(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            Bounds bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }
            return bounds;
        }

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }

        return new Bounds(obj.transform.position, Vector3.one * 0.1f);
    }

    private void CreateHover()
    {
        if (heldObject == null || hoverObject != null) return;

        hoverObject = Instantiate(heldObject.gameObject);
        hoverObject.name = heldObject.name + "_Hover";
        hoverObject.transform.SetParent(null);

        int hoverLayer = LayerMask.NameToLayer("Hover");
        if (hoverLayer != -1)
            SetLayerRecursively(hoverObject.transform, hoverLayer);

        foreach (var col in hoverObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
            col.isTrigger = true;
        }

        foreach (var body in hoverObject.GetComponentsInChildren<Rigidbody>())
        {
            body.isKinematic = true;
        }

        RemoveHoverOutline();
        ApplyHoverMaterial();
    }

    private void RemoveHoverOutline()
    {
        var outlines = hoverObject.GetComponentsInChildren<Outline>();
        for (int i = 0; i < outlines.Length; i++)
        {
            Destroy(outlines[i]);
        }
    }

    private void ApplyHoverMaterial()
    {
        if (hoverMaterial == null) return;

        var renderers = hoverObject.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            int count = renderer.sharedMaterials.Length;
            var materials = new Material[count];
            for (int i = 0; i < count; i++)
                materials[i] = hoverMaterial;

            renderer.sharedMaterials = materials;
        }
    }

    private void DestroyHover()
    {
        if (hoverObject == null) return;
        Destroy(hoverObject);
        hoverObject = null;
    }

    private void SetLayerRecursively(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        for (int i = 0; i < root.childCount; i++)
        {
            SetLayerRecursively(root.GetChild(i), layer);
        }
    }

    private bool TryGetInteractable(GameObject target, out IInteractable interactable)
    {
        interactable = FindInteractableOnObject(target);
        if (interactable != null) return true;

        Transform parent = target.transform.parent;
        while (parent != null)
        {
            interactable = FindInteractableOnObject(parent.gameObject);
            if (interactable != null) return true;
            parent = parent.parent;
        }

        return false;
    }

    private IInteractable FindInteractableOnObject(GameObject target)
    {
        MonoBehaviour[] behaviours = target.GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IInteractable interactable)
            {
                return interactable;
            }
        }

        return null;
    }
}
