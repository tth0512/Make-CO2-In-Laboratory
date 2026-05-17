using UnityEngine;
using UnityEngine.InputSystem;

public class ExperimentReadyState : BaseExperimentState
{
    public ExperimentReadyState(ExperimentReactionManager context) : base(context) { }

    public override void EnterState()
    {
        context.EnterExperimentMode();
        context.EnterReady();
    }

    public override void UpdateState()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleExitClick();
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryHandleValveClick();
        }
    }

    private void TryHandleValveClick()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (context.IsValveTarget(hit.transform))
            {
                context.TryTurnValve();
            }
        }
    }

    public override void ExitState()
    {
    }

    public override void HandleExitClick()
    {
        context.ExitExperimentMode();
    }
}
