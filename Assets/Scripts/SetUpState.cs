using UnityEngine;

public class SetUpState : ExperimentState
{
    public SetUpState(StateMachine stateMachine) : base(stateMachine)
    {
    }
    public override void EnterState()
    {
        Debug.Log("Entering SetUp State");
        // Thực hiện các thiết lập ban đầu ở đây
    }
    public override void ExitState()
    {
        Debug.Log("Exiting SetUp State");
        // Dọn dẹp hoặc reset các thiết lập nếu cần
    }
    public override void FrameUpdate()
    {
        // Cập nhật logic mỗi frame nếu cần
    }
}

