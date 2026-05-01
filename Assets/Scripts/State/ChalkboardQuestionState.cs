using UnityEngine;
using UnityEngine.InputSystem;

public class ChalkboardQuestionState : BaseChalkboardState
{
    public ChalkboardQuestionState(ChalkboardManager context) : base(context) { }

    public override void EnterState()
    {
        context.EnterQuestionMode();
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
        // cleanup handled by manager when transitioning to inactive
    }

    // Ghi đè hàm xử lý click chuột từ lớp cha
    public override void HandleAnswerClick(int answerIndex)
    {
        context.lastSelectedAnswer = answerIndex;
    }

    public override void HandleExitClick()
    {
        context.stateMachine.ChangeState<ChalkboardInactiveState>();
    }
}