public class ExperimentCompletedState : BaseExperimentState
{
    public ExperimentCompletedState(ExperimentReactionManager context) : base(context) { }

    public override void EnterState()
    {
        context.EnterCompleted();
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
    }
}
