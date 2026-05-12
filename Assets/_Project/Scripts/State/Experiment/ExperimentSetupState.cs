using UnityEngine;

public class ExperimentSetupState : BaseExperimentState
{
    public ExperimentSetupState(ExperimentReactionManager context) : base(context) { }

    public override void EnterState()
    {
        context.ExitExperimentMode();
    }

    public override void UpdateState()
    {
        int requiredTargets = 0;
        bool allPlaced = true;
        foreach (var target in context.targets)
        {
            if (target.interactableObject == null) continue;
            requiredTargets++;
            if (!context.installedTargets.Contains(target.interactableObject))
            {
                allPlaced = false;
            }
        }

        context.isExperimentReady = allPlaced && requiredTargets > 0;
        if (context.isExperimentReady)
        {
            context.stateMachine.ChangeState<ExperimentReadyState>();
            UnityEngine.Debug.Log("<color=green>[SUCCESS]</color> Experiment 1: Setup complete. Valve active.");
        }
    }

    public override void ExitState()
    {
    }
}
