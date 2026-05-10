using Cinemachine;
using UnityEngine;

public class ExperimentReactionManager : MonoBehaviour, IInteractable
{
    public ExperimentStateMachine<BaseExperimentState> stateMachine { get; private set; }

    [Header("References")]
    public CinemachineVirtualCamera experimentCam;

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
            stateMachine.ChangeState<ExperimentReadyState>();
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
