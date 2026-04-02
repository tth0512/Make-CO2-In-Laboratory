using System.Collections;
using UnityEngine;

public class CabinetDoorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform hinge;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    private bool isOpen;
    private bool isAnimating;
    private Quaternion closedRotation;

    private void Awake()
    {
        if (hinge == null)
        {
            hinge = transform;
        }

        closedRotation = hinge.localRotation;
    }

    public void Interact(PlayerPickUp interactor)
    {
        if (isAnimating) return;
        StartCoroutine(AnimateDoor(isOpen ? 0f : openAngle));
    }

    private IEnumerator AnimateDoor(float targetAngle)
    {
        isAnimating = true;

        Quaternion startRotation = hinge.localRotation;
        Quaternion targetRotation = closedRotation * Quaternion.AngleAxis(targetAngle, rotationAxis.normalized);

        float elapsed = 0f;
        float safeDuration = Mathf.Max(0.01f, duration);

        while (elapsed < safeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);
            t = t * t * (3f - 2f * t);
            hinge.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        hinge.localRotation = targetRotation;
        isOpen = !Mathf.Approximately(targetAngle, 0f);
        isAnimating = false;
    }
}
