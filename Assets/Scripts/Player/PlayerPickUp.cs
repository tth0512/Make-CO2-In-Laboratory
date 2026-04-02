using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickUp : MonoBehaviour
{
    private Transform LeftHand;
    private Transform RightHand;

    private void Awake()
    {
        LeftHand = transform.GetChild(2);
        RightHand = transform.GetChild(3);
    }

    public void OnTapInteract()
    {
        var hoveredObject = InteractionManager.Ins.GetHoveredObject();
        if (hoveredObject == null) return;

        if (TryGetInteractable(hoveredObject, out IInteractable interactable))
        {
            interactable.Interact(this);
            return;
        }

        TryPickUpObject(hoveredObject);

    }

    public void OnHoldInteract()
    {
        if (RightHand.childCount == 0) return;
        var curObj = RightHand.GetChild(0);
        if (curObj == null) return;

        Rigidbody body = curObj.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.isKinematic = false;
        }

        float keepY = curObj.position.y;
        Vector3 dropPosition = curObj.position;

        Camera cam = Camera.main;
        if (cam != null)
        {
            Ray centerRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Plane keepHeightPlane = new Plane(Vector3.up, new Vector3(0f, keepY, 0f));

            if (keepHeightPlane.Raycast(centerRay, out float enter))
            {
                dropPosition = centerRay.GetPoint(enter); // đổi X,Z theo tâm cam, giữ Y
            }
        }

        curObj.SetParent(null);
        curObj.position = dropPosition;
    }

    public void TryPickUpObject(GameObject target)
    {
        if (target == null) return;

        Rigidbody body = target.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.isKinematic = true;
        }

        target.transform.SetParent(RightHand);
        target.transform.localPosition = Vector3.zero;
        target.transform.localRotation = Quaternion.identity;
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
