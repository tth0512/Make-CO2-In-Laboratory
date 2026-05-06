using System;
using System.Collections.Generic;

public interface IExperimentState
{
    void EnterState();
    void UpdateState();
    void ExitState();
}

public class ExperimentStateMachine<TBaseState> where TBaseState : IExperimentState
{
    public TBaseState CurrentState { get; private set; }

    private readonly Dictionary<Type, TBaseState> stateCache = new Dictionary<Type, TBaseState>();

    public void AddState(TBaseState state)
    {
        Type type = state.GetType();
        if (!stateCache.ContainsKey(type))
            stateCache.Add(type, state);
    }

    public void Initialize<TState>() where TState : TBaseState
    {
        CurrentState = stateCache[typeof(TState)];
        CurrentState?.EnterState();
    }

    public void ChangeState<TState>() where TState : TBaseState
    {
        CurrentState?.ExitState();
        CurrentState = stateCache[typeof(TState)];
        CurrentState?.EnterState();
    }

    public void Update()
    {
        CurrentState?.UpdateState();
    }
}
