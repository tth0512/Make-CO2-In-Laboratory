using UnityEngine.InputSystem;

public class ExperimentReadyState : BaseExperimentState
{
    public ExperimentReadyState(ExperimentReactionManager context) : base(context) { }

    public override void EnterState()
    {
        context.EnterExperimentMode();
    }

    public override void UpdateState()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleExitClick();
        }
    }

    public override void ExitState()
    {
    }

    public override void HandleExitClick()
    {
        context.ExitExperimentMode();
    }
}
