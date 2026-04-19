using UnityEngine;

public abstract class BaseState<T>
{
    protected T context;

    public BaseState(T context)
    {
        this.context = context;
    }

    public virtual void EnterState() { }
    public virtual void ExitState() { }
    public virtual void FrameUpdate() { }
    public virtual void PhysicsUpdate() { }
    public virtual void AnimationTriggerEvent() { }
}
