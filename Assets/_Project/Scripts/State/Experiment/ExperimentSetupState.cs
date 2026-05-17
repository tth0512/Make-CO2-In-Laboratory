using UnityEngine;

public class ExperimentSetupState : BaseExperimentState
{
    public ExperimentSetupState(ExperimentReactionManager context) : base(context) { }

    public override void EnterState()
    {
        context.ExitExperimentMode();
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
    }
}
