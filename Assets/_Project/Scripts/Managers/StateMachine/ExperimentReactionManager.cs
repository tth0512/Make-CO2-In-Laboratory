using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExperimentReactionManager : MonoBehaviour, IInteractable
{
    public ExperimentStateMachine<BaseExperimentState> stateMachine { get; private set; }

    [Header("References")]
    public CinemachineVirtualCamera experimentCam;
    [SerializeField] private Transform valveHead;
    [SerializeField] private ParticleSystem bubbles;
    [SerializeField] private ParticleSystem gasFlow;
    [SerializeField] private GameObject caoObject;
    [SerializeField] private GameObject caco3Object;
    [SerializeField] private TMP_Text scaleDisplay;
    [SerializeField] private Canvas readyStateCanvas;

    [Header("Placement Validation")]
    public bool isExperimentReady;
    public List<string> installedTargets = new List<string>();
    private int currentInstallingIndex = 0;


    private PlayerManager playerManager;
    private float lastScaleValue = -1f;

    private void Awake()
    {
        playerManager = PlayerManager.Instance;
        stateMachine = new ExperimentStateMachine<BaseExperimentState>();
        stateMachine.AddState(new ExperimentSetupState(this));
        stateMachine.AddState(new ExperimentReadyState(this));
        stateMachine.AddState(new ExperimentReactingState(this));
        stateMachine.AddState(new ExperimentPausedState(this));
        stateMachine.AddState(new ExperimentCompletedState(this));
    }

    private void Start()
    {
        stateMachine.Initialize<ExperimentSetupState>();
        AddInstalledTarget();
    }

    private void AddInstalledTarget()
    {
        installedTargets.Add("Lab_Scale");
        installedTargets.Add("GasWashBottle");
        installedTargets.Add("CaO");
        installedTargets.Add("CO2");
        installedTargets.Add("Valve_body");
        installedTargets.Add("Valve_Head");
        installedTargets.Add("ConnectorHose");
    }

    private void Update()
    {
        stateMachine.Update();
    }

    public void EnterSetup() { }
    public void EnterReady()
    {
        if (readyStateCanvas != null)
        {
            readyStateCanvas.enabled = true;
        }

        SetScaleDisplay(2f);
    }
    public void EnterReacting() { }
    public void UpdateReacting() { }
    public void ExitReacting() { }
    public void EnterPaused() { }
    public void ExitPaused() { }
    public void EnterCompleted() { }

    private void ChangeMaterial(GameObject init, GameObject target)
    {
        List<MeshRenderer> initRenderes = new();
        if (init.GetComponent<MeshRenderer>() != null)
        {
            initRenderes.Add(init.GetComponent<MeshRenderer>());
        }

        foreach (Transform child in init.transform)
        {
            var childMeshRenderer = child.GetComponent<MeshRenderer>();
            if (childMeshRenderer != null)
                initRenderes.Add(childMeshRenderer);
        }

        List<MeshRenderer> targetRenderes = new();
        if (target.GetComponent<MeshRenderer>() != null)
        {
            targetRenderes.Add(target.GetComponent<MeshRenderer>());
        }
        foreach(Transform child in target.transform) 
        {
            var childMeshRenderer = child.GetComponent<MeshRenderer>();
            if (childMeshRenderer != null)
                targetRenderes.Add(childMeshRenderer);
        }

        var idx = 0;
        while(idx < initRenderes.Count)
        {
            initRenderes[idx].material = targetRenderes[idx].material;
            idx++;
        }
    }

    public void Interact(PlayerPickUp interactor)
    {
        Debug.Log("Current State: " + stateMachine.CurrentState.GetType().Name);
        if (stateMachine.CurrentState is ExperimentSetupState)
        {
            if (isExperimentReady)
            {
                stateMachine.ChangeState<ExperimentReadyState>();
                Debug.Log("<color=green>[SUCCESS]</color> Experiment 1: Setup complete. Valve active.");
            }
            else
            {
                Debug.Log("Installing Experiment Target...");
                var heldObject = PlayerManager.Instance.GetHeldObject();

                if (heldObject == null) return;

                Debug.Log($"[ExperimentReactionManager] Held Object: {(heldObject != null ? heldObject.name : "None")}");

                if (heldObject.name == installedTargets[currentInstallingIndex])
                {
                    currentInstallingIndex++;
                    var toBeInstalledObject = transform.Find(heldObject.name);
                    ChangeMaterial(toBeInstalledObject.gameObject, heldObject);
                    interactor.DestroyHeldObject();

                    Debug.Log($"[ExperimentReactionManager] Installed: {heldObject.name}");
                }

                if (currentInstallingIndex >= installedTargets.Count)
                {
                    isExperimentReady = true;
                    stateMachine.ChangeState<ExperimentReadyState>();
                    Debug.Log("<color=yellow>[INFO]</color> All targets installed. Interact with the valve to start the experiment.");
                }
            }
        }
    }

    public void EnterExperimentMode()
    {

        if (experimentCam != null)
        {
            experimentCam.Priority = 20;
        }

        playerManager.DisablePlayer();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitExperimentMode()
    {

        if (experimentCam != null)
        {
            experimentCam.Priority = 0;
        }

        if (playerManager != null)
            playerManager.EnablePlayer();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void TryTurnValve()
    {
        if (stateMachine.CurrentState is not ExperimentReadyState) return;
        stateMachine.ChangeState<ExperimentReactingState>();
        StartCoroutine(RunReaction());
    }

    public bool IsValveTarget(Transform target)
    {
        if (valveHead == null || target == null) return false;
        return target == valveHead || target.IsChildOf(valveHead) || valveHead.IsChildOf(target);
    }

    private void SetScaleDisplay(float value)
    {
        if (scaleDisplay == null)
        {
            return;
        }

        if (Mathf.Abs(value - lastScaleValue) > 0.001f)
        {
            scaleDisplay.text = value.ToString("0.00");
            lastScaleValue = value;
        }
    }

    private IEnumerator RunReaction()
    {
        Debug.Log("Starting CO2 reaction...");

        float dur = 1.0f; float elapsed = 0;
        Quaternion startRot = valveHead.localRotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0, 0, 90);
        while (elapsed < dur) { valveHead.localRotation = Quaternion.Lerp(startRot, targetRot, elapsed / dur); elapsed += Time.deltaTime; yield return null; }
        valveHead.localRotation = targetRot;

        if (bubbles != null) { var em = bubbles.emission; em.enabled = true; bubbles.Play(); }
        if (gasFlow != null) { var em = gasFlow.emission; em.enabled = true; gasFlow.Play(); }
        yield return new WaitForSeconds(2f);

        float t = 0; Vector3 startScale = caoObject.transform.localScale; Vector3 endScale = caco3Object.transform.localScale;
        caco3Object.SetActive(true);
        while (t < 1.0f)
        {
            caoObject.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            caco3Object.transform.localScale = Vector3.Lerp(Vector3.zero, endScale, t);
            t += Time.deltaTime / 3f; yield return null;
        }
        caoObject.SetActive(false);

        yield return new WaitForSeconds(1f);
        if (bubbles != null) { var em = bubbles.emission; em.enabled = false; }
        if (gasFlow != null) { var em = gasFlow.emission; em.enabled = false; }
        Debug.Log("Reaction Complete.");
        SetScaleDisplay(5f);
        stateMachine.ChangeState<ExperimentCompletedState>();
    }
}
