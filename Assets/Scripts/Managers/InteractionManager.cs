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
    }

    private void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject objectHitByRaycast = hit.transform.gameObject;

            if (objectHitByRaycast != null)
            {
                //Debug.Log("Detected object: " + objectHitByRaycast.name);
                if (objectHitByRaycast.CompareTag("Interactable") || objectHitByRaycast.CompareTag("showcase"))
                {
                    if (hoveredObject && objectHitByRaycast != hoveredObject)
                    {
                        hoveredObject.GetComponent<Outline>().enabled = false;
                    }

                    objectHitByRaycast.GetComponent<Outline>().enabled = true;
                    hoveredObject = objectHitByRaycast;
                }
                else if (objectHitByRaycast.CompareTag("Cabinet"))
                {
                    if (hoveredObject && objectHitByRaycast != hoveredObject)
                    {
                        hoveredObject.GetComponent<Outline>().enabled = false;
                    }

                    objectHitByRaycast.GetComponent<Outline>().enabled = true;
                    hoveredObject = objectHitByRaycast;
                }
                else
                {
                    if (hoveredObject)
                    {
                        hoveredObject.GetComponent<Outline>().enabled = false;
                        hoveredObject = null;
                    }
                }
            }
        }
        else
        {
            hoveredObject = null;
        }
    }
}
