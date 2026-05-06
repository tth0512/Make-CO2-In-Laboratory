using UnityEngine;

public class ExperimentReactionManager : MonoBehaviour
{
    public ExperimentStateMachine<BaseExperimentState> stateMachine { get; private set; }

    private void Awake()
    {
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
}
