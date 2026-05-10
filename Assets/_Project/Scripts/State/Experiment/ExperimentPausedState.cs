public class ExperimentPausedState : BaseExperimentState
{
    public ExperimentPausedState(ExperimentReactionManager context) : base(context) { }

    public override void EnterState()
    {
        context.EnterPaused();
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
        context.ExitPaused();
    }
}
