using UnityEngine;

public class ValveInteractable : MonoBehaviour, IInteractable
{
    public ExperimentManager experimentManager;

    public void Interact(PlayerPickUp interactor)
    {
        if (experimentManager != null)
        {
            experimentManager.TryTurnValve();
        }
    }
}
