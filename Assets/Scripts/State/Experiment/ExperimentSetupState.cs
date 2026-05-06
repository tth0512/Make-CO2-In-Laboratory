public class ExperimentSetupState : BaseExperimentState
{
    public ExperimentSetupState(ExperimentReactionManager context) : base(context) { }

    public override void EnterState()
    {
        context.EnterSetup();
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
    }
}
