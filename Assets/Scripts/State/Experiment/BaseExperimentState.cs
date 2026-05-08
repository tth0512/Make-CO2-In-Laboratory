using UnityEngine;

public abstract class BaseExperimentState : IExperimentState
{
    protected ExperimentReactionManager context;

    protected BaseExperimentState(ExperimentReactionManager context)
    {
        this.context = context;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();

    public virtual void HandleAnswerClick(int answerIndex)
    {
        // Mặc định không làm gì cả
    }

    // Gọi khi người chơi click nút "Thoát" hoặc ấn ESC trên bảng
    public virtual void HandleExitClick()
    {
        // Mặc định không làm gì cả
    }
}
