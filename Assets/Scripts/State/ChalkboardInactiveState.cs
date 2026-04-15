using UnityEngine;

public class ChalkboardInactiveState : BaseChalkboardState
{
    public ChalkboardInactiveState(ChalkboardManager context) : base(context) { }

    public override void EnterState()
    {
        context.ExitQuestionMode();
    }

    public override void UpdateState() { }
    public override void ExitState() { }
}