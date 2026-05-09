using UnityEngine;
using UnityEngine.InputSystem;

public class ChalkboardQuestionState : BaseChalkboardState
{
    public ChalkboardQuestionState(ChalkboardManager context) : base(context) { }

    public override void EnterState()
    {
        context.EnterQuestionMode();
        AudioManager.Ins.PauseBackgroundMusic();
        AudioManager.Ins.PlayBackgroundMusic(0); // Phát nhạc nền khi vào trạng thái câu hỏi
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
        AudioManager.Ins.PauseBackgroundMusic(); // Dừng nhạc nền khi rời khỏi trạng thái câu hỏi
        AudioManager.Ins.PlayBackgroundMusic(1); // Quay lại nhạc nền chung khi rời khỏi trạng thái câu hỏi
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