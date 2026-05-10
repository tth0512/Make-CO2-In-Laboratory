using UnityEngine;

public class PickupInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject pickupTarget;

    public void Interact(PlayerPickUp interactor)
    {
        if (interactor == null) return;

        GameObject target = pickupTarget != null ? pickupTarget : gameObject;
        interactor.TryPickUpObject(target);
    }
}
