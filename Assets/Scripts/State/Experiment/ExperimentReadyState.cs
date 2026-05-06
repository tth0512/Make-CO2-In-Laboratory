public class ExperimentReadyState : BaseExperimentState
{
    public ExperimentReadyState(ExperimentReactionManager context) : base(context) { }

    public override void EnterState()
    {
        context.EnterReady();
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
    }
}
