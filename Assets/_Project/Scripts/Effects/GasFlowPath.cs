using UnityEngine;

public class GasFlowPath : MonoBehaviour
{
    public LineRenderer path;
    public ParticleSystem ps;
    public bool debugMode = false;

    private ParticleSystem.Particle[] particles;

    void Start()
    {
        if (ps == null) ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            particles = new ParticleSystem.Particle[ps.main.maxParticles];
            // Ensure particles are set to World space for easier script control
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSpeed = 0;
        }
    }

    void LateUpdate()
    {
        if (path == null || ps == null || path.positionCount < 2) return;

        int numParticlesAlive = ps.GetParticles(particles);
        if (numParticlesAlive == 0) return;

        float pathLength = 0;
        for (int i = 0; i < path.positionCount - 1; i++)
            pathLength += Vector3.Distance(path.GetPosition(i), path.GetPosition(i + 1));

        for (int i = 0; i < numParticlesAlive; i++)
        {
            float normalizedLifetime = 1.0f - (particles[i].remainingLifetime / particles[i].startLifetime);
            particles[i].position = GetPointOnPath(normalizedLifetime, pathLength);
        }

        ps.SetParticles(particles, numParticlesAlive);
    }

    private Vector3 GetPointOnPath(float t, float totalLength)
    {
        float targetDist = t * totalLength;
        float currentDist = 0;

        for (int i = 0; i < path.positionCount - 1; i++)
        {
            Vector3 p1 = path.GetPosition(i);
            Vector3 p2 = path.GetPosition(i + 1);
            float segDist = Vector3.Distance(p1, p2);
            
            if (currentDist + segDist >= targetDist)
            {
                float segT = (targetDist - currentDist) / segDist;
                return Vector3.Lerp(p1, p2, segT);
            }
            currentDist += segDist;
        }

        return path.GetPosition(path.positionCount - 1);
    }
}
