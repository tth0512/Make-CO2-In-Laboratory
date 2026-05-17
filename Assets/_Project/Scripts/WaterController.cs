using UnityEngine;

public class WaterController : MonoBehaviour
{
    [Range(0, 1)]
    public float fillLevel = 0.2f;
    public bool isBubbling = false;
    
    public ParticleSystem bubbleSystem;
    public Material waterMaterial;

    private float currentFill = 0.2f;
    private float containerHeight = 0.14f; // Based on Jar extents 0.07

    void Update()
    {
        // Smoothly adjust fill level
        currentFill = Mathf.Lerp(currentFill, fillLevel, Time.deltaTime);
        
        // Update scale and position based on fill
        // Full height is ~0.14. Scale Y is height/2.
        float height = currentFill * containerHeight;
        transform.localScale = new Vector3(transform.localScale.x, height / 2f, transform.localScale.z);
        transform.localPosition = new Vector3(0, height / 2f, 0);

        // Manage bubbles
        if (bubbleSystem != null)
        {
            var emission = bubbleSystem.emission;
            emission.enabled = isBubbling;
        }

        // Update shader properties if needed
        if (waterMaterial != null)
        {
            // waterMaterial.SetFloat("_FillAmount", currentFill);
        }
    }

    public void SetBubbling(bool active)
    {
        isBubbling = active;
    }
}
