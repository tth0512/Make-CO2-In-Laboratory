using UnityEngine;

[DefaultExecutionOrder(-200)]
public class InteractionManager : MonoBehaviour
{
    private static InteractionManager _instance { get; set; }
    public static InteractionManager Ins
    {
        get
        {
            return _instance;
        }
    }
    private GameObject hoveredObject;
    private GameObject hoverSoundObject;
    private Camera mainCamera;

    [SerializeField] private LayerMask raycastMask = ~0;
    public GameObject GetHoveredObject() => hoveredObject;
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        _instance = this;
        mainCamera = Camera.main;
    }

    private void Start()
    {
        if (_instance == null) _instance = this;
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastMask, QueryTriggerInteraction.Ignore))
        {
            GameObject objectHitByRaycast = hit.transform.gameObject;

            if (objectHitByRaycast != null)
            {
                GameObject rootInteractable = FindInteractableRoot(objectHitByRaycast);

                if (rootInteractable != null && rootInteractable != hoveredObject)
                {
                    if (rootInteractable.CompareTag("Interactable") || rootInteractable.CompareTag("ExperimentStuff") || rootInteractable.CompareTag("Experiment"))
                    {
                        hoveredObject = rootInteractable;
                        if (AudioManager.Ins != null && hoverSoundObject != hoveredObject)
                        {
                            AudioManager.Ins.PlayBubbleHoverSound();
                            hoverSoundObject = hoveredObject;
                        }
                    }
                    else if (rootInteractable.CompareTag("showcase") || rootInteractable.CompareTag("Cabinet"))
                    {
                        hoveredObject = rootInteractable;
                        if (AudioManager.Ins != null && hoverSoundObject != hoveredObject)
                        {
                            AudioManager.Ins.PlayBubbleHoverSound();
                            hoverSoundObject = hoveredObject;
                        }
                    }
                }
                else if (rootInteractable == null)
                {
                    ClearHover();
                }
            }
        }
        else
        {
            ClearHover();
        }
        }

        private GameObject FindInteractableRoot(GameObject obj)
        {
        Transform current = obj.transform;
        while (current != null)
        {
            if (current.CompareTag("Interactable") || current.CompareTag("showcase") || current.CompareTag("Cabinet")
                || current.CompareTag("Experiment") || current.CompareTag("ExperimentStuff"))   
            {
                return current.gameObject;
            }
            current = current.parent;
        }
        return null;
        }

        private void ClearHover()
        {
        hoveredObject = null;
        hoverSoundObject = null;
        }
        }
