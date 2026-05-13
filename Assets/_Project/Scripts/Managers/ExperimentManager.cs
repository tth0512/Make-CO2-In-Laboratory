using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteAlways]
public class ExperimentManager : MonoBehaviour
{
    private static List<ExperimentManager> _instances = new List<ExperimentManager>();
    public static IReadOnlyList<ExperimentManager> Instances => _instances;

    [Header("Experiment Components")]
    public Transform valveHead;
    public ParticleSystem bubbles;
    public ParticleSystem gasFlow;
    public GameObject activeObject; 
    public GameObject resultObject; 
    
    [Header("Placement Validation")]
    public float distanceThreshold = 0.3f; 
    public float angleThreshold = 20f;
    public float visualRange = 2.5f; 
    public List<PlacementTarget> targets = new List<PlacementTarget>();

    [System.Serializable]
    public struct PlacementTarget
    {
        public GameObject interactableObject;
        public Vector3 targetPosition;
        public Vector3 targetRotation;
    }

    [Header("State")]
    public bool isExperimentReady = false;
    public bool isReacting = false;
    public bool isCompleted = false;

    [Header("Visual Feedback")]
    public Material ghostMaterial;
    public Material ghostGreenMaterial;
    
    [Header("Debug")]
    public bool showDebugVisuals = true;

    private Dictionary<GameObject, GhostIndicator> indicators = new Dictionary<GameObject, GhostIndicator>();
    private float lastLogTime = 0f;

    private void OnEnable()
    {
        if (!_instances.Contains(this)) _instances.Add(this);
        if (Application.isPlaying)
            StartCoroutine(SubscribeToEvents());
    }

    private void OnDisable()
    {
        _instances.Remove(this);
        if (PlayerManager.Instance != null)
        {
            var pickup = PlayerManager.Instance.GetComponentInChildren<PlayerPickUp>();
            if (pickup != null) pickup.OnObjectPlaced -= OnObjectPlaced;
        }
    }

    private IEnumerator SubscribeToEvents()
    {
        while (PlayerManager.Instance == null) yield return null;
        PlayerPickUp pickup = PlayerManager.Instance.GetComponentInChildren<PlayerPickUp>();
        while (pickup == null) { pickup = PlayerManager.Instance.GetComponentInChildren<PlayerPickUp>(); yield return null; }
        pickup.OnObjectPlaced += OnObjectPlaced;
    }

    private void OnObjectPlaced(GameObject obj)
    {
        CheckPlacement();
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            if (resultObject != null) resultObject.SetActive(false);
            if (bubbles != null) { var emission = bubbles.emission; emission.enabled = false; }
        }
        SetupIndicators();
        if (gasFlow == null)
        {
            Transform gfe = transform.Find("GasFlowEffect");
            if (gfe != null) gasFlow = gfe.GetComponent<ParticleSystem>();
        }
        CheckPlacement();
    }

    private void SetupIndicators()
    {
        indicators.Clear();
        GhostIndicator[] existing = GetComponentsInChildren<GhostIndicator>(true);
        foreach (var gi in existing)
        {
            if (gi.targetObject != null)
            {
                if (!indicators.ContainsKey(gi.targetObject))
                    indicators.Add(gi.targetObject, gi);
            }
        }
    }

    private void Update()
    {
        if (indicators.Count == 0 && targets.Count > 0) SetupIndicators();

        if (Application.isPlaying)
        {
            UpdateIndicatorsPlayMode();
        }
        else
        {
            UpdateIndicatorsEditorMode();
            SyncTargetsWithGhosts();
        }
    }

    private void SyncTargetsWithGhosts()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            if (target.interactableObject == null) continue;

            if (indicators.TryGetValue(target.interactableObject, out GhostIndicator ghost))
            {
                if (Vector3.Distance(target.targetPosition, ghost.transform.position) > 0.001f ||
                    Quaternion.Angle(Quaternion.Euler(target.targetRotation), ghost.transform.rotation) > 0.1f)
                {
                    target.targetPosition = ghost.transform.position;
                    target.targetRotation = ghost.transform.eulerAngles;
                    targets[i] = target;
                }
            }
        }
    }

    private void UpdateIndicatorsPlayMode()
    {
        PlayerPickUp pickup = null;
        if (PlayerManager.Instance != null) pickup = PlayerManager.Instance.GetComponentInChildren<PlayerPickUp>();
        else pickup = Object.FindAnyObjectByType<PlayerPickUp>();

        if (pickup == null) return;

        GameObject held = pickup.GetHeldObject();

        foreach (var kvp in indicators)
        {
            GameObject obj = kvp.Key;
            GhostIndicator ghost = kvp.Value;
            if (obj == null || ghost == null) continue;

            float dist = Vector3.Distance(obj.transform.position, ghost.transform.position);
            
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            bool isHeld = (held == obj);
            bool isAtTarget = dist < distanceThreshold * 1.5f; 
            bool isKinematic = (rb != null && rb.isKinematic);

            bool isPlaced = !isHeld && (dist < distanceThreshold || (isKinematic && isAtTarget));
            
            ghost.Show(!isPlaced);

            if (!isPlaced)
            {
                bool isMatch = IsObjectMatch(held, obj);
                bool isNear = isMatch && dist < visualRange;
                ghost.SetState(isNear);
            }
        }
    }

    private void UpdateIndicatorsEditorMode()
    {
#if UNITY_EDITOR
        GameObject selected = UnityEditor.Selection.activeGameObject;
        foreach (var kvp in indicators)
        {
            GameObject obj = kvp.Key;
            GhostIndicator ghost = kvp.Value;
            if (obj == null || ghost == null) continue;

            float dist = Vector3.Distance(obj.transform.position, ghost.transform.position);
            bool isPlaced = dist < distanceThreshold;
            ghost.Show(!isPlaced);

            if (!isPlaced)
            {
                bool isSelected = (selected == obj || (selected != null && selected.transform.IsChildOf(obj.transform)));
                ghost.SetState(isSelected && dist < 1.0f);
            }
        }
#endif
    }

    private bool IsObjectMatch(GameObject held, GameObject target)
    {
        if (held == null || target == null) return false;
        if (held == target) return true;
        
        string hN = held.name.ToLower().Replace("_hover", "").Replace("(clone)", "").Trim();
        string tN = target.name.ToLower().Replace("_hover", "").Replace("(clone)", "").Trim();
        
        return hN == tN || hN.Contains(tN) || tN.Contains(hN);
    }

    public static bool TryGetAnySnapTarget(GameObject heldObject, out Vector3 pos, out Vector3 rot)
    {
        pos = Vector3.zero; rot = Vector3.zero;
        foreach (var manager in _instances)
        {
            if (manager.TryGetSnapTarget(heldObject, out pos, out rot)) return true;
        }
        return false;
    }

    public bool TryGetSnapTarget(GameObject heldObject, out Vector3 pos, out Vector3 rot)
    {
        pos = Vector3.zero; rot = Vector3.zero;
        if (heldObject == null) return false;

        foreach (var kvp in indicators)
        {
            if (IsObjectMatch(heldObject, kvp.Key))
            {
                float dist = Vector3.Distance(heldObject.transform.position, kvp.Value.transform.position);
                if (dist < visualRange)
                {
                    pos = kvp.Value.transform.position;
                    rot = kvp.Value.transform.eulerAngles;
                    return true;
                }
            }
        }
        return false;
    }

    public void CheckPlacement()
    {
        bool allCorrect = true;
        foreach (var target in targets)
        {
            if (target.interactableObject == null) continue;
            
            Vector3 goalPos = target.targetPosition;
            Vector3 goalRot = target.targetRotation;

            if (indicators.TryGetValue(target.interactableObject, out GhostIndicator ghost))
            {
                goalPos = ghost.transform.position;
                goalRot = ghost.transform.eulerAngles;
            }

            float dist = Vector3.Distance(target.interactableObject.transform.position, goalPos);
            float angle = Quaternion.Angle(target.interactableObject.transform.rotation, Quaternion.Euler(goalRot));

            Rigidbody rb = target.interactableObject.GetComponent<Rigidbody>();
            bool isFixed = rb != null && rb.isKinematic;

            if (!(dist < distanceThreshold && angle < angleThreshold && isFixed)) { allCorrect = false; }
        }

        isExperimentReady = allCorrect;
    }

    public void TryTurnValve()
    {
        if (isCompleted || isReacting) return;
        if (!isExperimentReady) { Debug.Log("<color=yellow>[WARNING]</color> Setup not complete."); return; }
        StartCoroutine(RunReaction());
    }

    private IEnumerator RunReaction()
    {
        isReacting = true;
        float dur = 1.0f; float elapsed = 0;
        Quaternion startRot = valveHead.localRotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0, 0, 90);
        while (elapsed < dur) { valveHead.localRotation = Quaternion.Lerp(startRot, targetRot, elapsed / dur); elapsed += Time.deltaTime; yield return null; }
        valveHead.localRotation = targetRot;

        if (bubbles != null) { var em = bubbles.emission; em.enabled = true; bubbles.Play(); }
        if (gasFlow != null) { var em = gasFlow.emission; em.enabled = true; gasFlow.Play(); }
        yield return new WaitForSeconds(2f);

        if (activeObject != null && resultObject != null)
        {
            float t = 0; 
            Vector3 startScale = activeObject.transform.localScale; 
            Vector3 endScale = resultObject.transform.localScale;
            resultObject.SetActive(true);
            while(t < 1.0f) { 
                activeObject.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                resultObject.transform.localScale = Vector3.Lerp(Vector3.zero, endScale, t);
                t += Time.deltaTime / 3f; yield return null; 
            }
            activeObject.SetActive(false);
        }
        
        yield return new WaitForSeconds(1f);
        if (bubbles != null) { var em = bubbles.emission; em.enabled = false; }
        if (gasFlow != null) { var em = gasFlow.emission; em.enabled = false; }
        isReacting = false; isCompleted = true;
    }
}
