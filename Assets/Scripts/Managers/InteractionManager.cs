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
    private Outline hoveredOutline;
    private GameObject hoverSoundObject;
    private Camera mainCamera;
    [SerializeField] private LayerMask raycastMask = ~0;
    public GameObject GetHoveredObject() => hoveredObject;
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }

        mainCamera = Camera.main;
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
                bool isHoverable = objectHitByRaycast.CompareTag("Interactable")
                    || objectHitByRaycast.CompareTag("showcase")
                    || objectHitByRaycast.CompareTag("Cabinet");

                if (isHoverable && objectHitByRaycast != hoveredObject)
                {
                    if (hoveredOutline != null)
                        hoveredOutline.enabled = false;

                    hoveredObject = objectHitByRaycast;
                    hoveredOutline = hoveredObject.GetComponent<Outline>();
                    if (hoveredOutline != null)
                        hoveredOutline.enabled = true;
                    if (AudioManager.Ins != null && hoverSoundObject != hoveredObject)
                    {
                        AudioManager.Ins.PlayBubbleHoverSound();
                        hoverSoundObject = hoveredObject;
                    }
                }
                else if (!isHoverable && hoveredOutline != null)
                {
                    hoveredOutline.enabled = false;
                    hoveredObject = null;
                    hoveredOutline = null;
                    hoverSoundObject = null;
                }
            }
        }
        else
        {
            if (hoveredOutline != null)
                hoveredOutline.enabled = false;
            hoveredObject = null;
            hoveredOutline = null;
            hoverSoundObject = null;
        }
    }
}
