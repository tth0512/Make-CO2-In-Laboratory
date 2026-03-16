using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Ins { get; set; }
    private GameObject hoveredObject;

    public GameObject GetHoveredObject() => hoveredObject;
    private void Awake()
    {
        if (Ins != null && Ins != this)
        {
            Destroy(this);
        }
        else
        {
            Ins = this;
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
                if (objectHitByRaycast.CompareTag("Glass"))
                {
                    hoveredObject = objectHitByRaycast;
                    hoveredObject.GetComponent<Outline>().enabled = true;
                }
                else
                {
                    if (hoveredObject)
                    {
                        hoveredObject.GetComponent<Outline>().enabled = false;
                    }
                }
            }

        }
    }
}
