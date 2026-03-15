using UnityEngine;

public class StateMachine
{
    public ExperimentState CurrentState { get; private set; }
    public void Initialize(ExperimentState startingState)
    {
        CurrentState = startingState;
        startingState.EnterState();
    }

    public void ChangeState(ExperimentState newState)
    {
        CurrentState.ExitState();
        CurrentState = newState;
        newState.EnterState();
    }
}
