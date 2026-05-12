using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentReactionManager : MonoBehaviour, IInteractable
{
    public ExperimentStateMachine<BaseExperimentState> stateMachine { get; private set; }

    [Header("References")]
    public CinemachineVirtualCamera experimentCam;

    [Header("Placement Validation")]
    public bool isExperimentReady;
    public List<string> installedTargets = new List<string>();
    private int currentInstallingIndex = 0;


    private PlayerManager playerManager;

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
        installedTargets.Add("GasWashBottle");
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
    public void EnterReady() { }
    public void EnterReacting() { }
    public void UpdateReacting() { }
    public void ExitReacting() { }
    public void EnterPaused() { }
    public void ExitPaused() { }
    public void EnterCompleted() { }

    public void Interact(PlayerPickUp interactor)
    {
        if (stateMachine.CurrentState is ExperimentSetupState)
        {
            if (isExperimentReady)
            {
                stateMachine.ChangeState<ExperimentReadyState>();
                Debug.Log("<color=green>[SUCCESS]</color> Experiment 1: Setup complete. Valve active.");
            }
            else
            {
                
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
}
