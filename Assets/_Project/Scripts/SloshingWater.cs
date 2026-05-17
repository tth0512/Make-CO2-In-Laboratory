using UnityEngine;

[ExecuteAlways]
public class SloshingWater : MonoBehaviour
{
    public Transform container;
    public float sloshSpeed = 10f;
    public float maxSlosh = 10f; // Max angle in degrees
    public float damping = 5f;
    
    private Vector3 lastPos;
    private Vector2 sloshAmount;
    private Vector2 sloshVelocity;

    void OnEnable()
    {
        if (container == null) container = transform.parent;
        if (container != null) lastPos = container.position;
        sloshAmount = Vector2.zero;
        sloshVelocity = Vector2.zero;
    }

    void Update()
    {
        if (container == null || Time.deltaTime <= 0) return;

        // Calculate change in position (delta)
        Vector3 deltaPos = container.position - lastPos;
        lastPos = container.position;

        // Force based on movement (acceleration equivalent)
        // Using deltaPos directly as a force impulse
        Vector2 force = new Vector2(deltaPos.x, deltaPos.z) * (1f / Mathf.Max(Time.deltaTime, 0.001f));
        
        // Cap the force to prevent spikes
        force = Vector2.ClampMagnitude(force, 100f);

        // Physics integration
        sloshVelocity += force * Time.deltaTime;
        sloshAmount += sloshVelocity * Time.deltaTime;
        
        // Spring back to center (restoring force)
        sloshVelocity -= sloshAmount * sloshSpeed * Time.deltaTime;
        // Damping
        sloshVelocity -= sloshVelocity * damping * Time.deltaTime;

        // Safety: Prevent NaN or Infinity from breaking Quaternion.Euler
        if (!float.IsFinite(sloshAmount.x)) sloshAmount.x = 0;
        if (!float.IsFinite(sloshAmount.y)) sloshAmount.y = 0;

        // Clamp to user defined limits
        sloshAmount.x = Mathf.Clamp(sloshAmount.x, -maxSlosh, maxSlosh);
        sloshAmount.y = Mathf.Clamp(sloshAmount.y, -maxSlosh, maxSlosh);

        // Apply rotation - use a safe method
        transform.localRotation = Quaternion.Euler(sloshAmount.y, 0, -sloshAmount.x);
    }
}
