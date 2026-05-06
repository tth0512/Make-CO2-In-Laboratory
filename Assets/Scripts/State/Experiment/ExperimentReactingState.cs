public class ExperimentReactingState : BaseExperimentState
{
    public ExperimentReactingState(ExperimentReactionManager context) : base(context) { }

    public override void EnterState()
    {
        context.EnterReacting();
    }

    public override void UpdateState()
    {
        context.UpdateReacting();
    }

    public override void ExitState()
    {
        context.ExitReacting();
    }
}
