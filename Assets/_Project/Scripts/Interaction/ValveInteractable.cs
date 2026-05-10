using UnityEngine;

public class ValveInteractable : MonoBehaviour, IInteractable
{
    public void Interact(PlayerPickUp interactor)
    {
        if (Experiment1Manager.Instance != null)
        {
            Experiment1Manager.Instance.TryTurnValve();
        }
    }
}
