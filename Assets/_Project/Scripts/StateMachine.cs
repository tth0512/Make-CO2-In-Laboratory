using System;
using System.Collections.Generic;
using UnityEngine;

// TBaseState là một Ràng buộc, yêu cầu kiểu dữ liệu truyền vào phải là một State.
public class StateMachine<TBaseState> where TBaseState : IState
{
    public TBaseState CurrentState { get; private set; }

    // Dictionary dùng để cache (lưu trữ sẵn) các State, khắc phục lỗi dùng từ khóa "new"
    private Dictionary<Type, TBaseState> _stateCache = new Dictionary<Type, TBaseState>();

    // Hàm thêm State vào bộ nhớ đệm (Chỉ gọi 1 lần khi Start)
    public void AddState(TBaseState state)
    {
        Type type = state.GetType();
        if (!_stateCache.ContainsKey(type))
        {
            _stateCache.Add(type, state);
        }
    }

    // Hàm Initialize dùng Generic
    public void Initialize<TState>() where TState : TBaseState
    {
        CurrentState = _stateCache[typeof(TState)];
        CurrentState?.EnterState();
    }

    // Hàm ChangeState dùng Generic thay vì tham số object
    public void ChangeState<TState>() where TState : TBaseState
    {
        CurrentState?.ExitState();

        CurrentState = _stateCache[typeof(TState)];

        CurrentState?.EnterState();
    }

    public void Update()
    {
        CurrentState?.UpdateState();
    }
}