using Cinemachine;
using UnityEngine;

public class CinemachinePOVExtension : CinemachineExtension
{
    [SerializeField]
    private float horizontalSpeed = 10f;

    [SerializeField]
    private float verticalSpeed = 10f;

    [SerializeField]
    private float clampAngle = 80f;

    private InputManager inputManager;
    private Vector3 startingRotation;

    protected override void Awake()
    {
        inputManager = InputManager.Instance;
        base.Awake();
    }

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (inputManager == null)
            inputManager = InputManager.Instance;

        if (vcam.Follow && inputManager != null)
        {
            if (stage == CinemachineCore.Stage.Aim)
            {
                if (startingRotation == Vector3.zero)
                    startingRotation = transform.localRotation.eulerAngles;

                Vector2 deltaInput = inputManager.GetMouseDelta();
                startingRotation.x += deltaInput.x * verticalSpeed * deltaTime;
                startingRotation.y -= deltaInput.y * horizontalSpeed * deltaTime;
                startingRotation.y = Mathf.Clamp(startingRotation.y, -clampAngle, clampAngle);
                state.RawOrientation = Quaternion.Euler(startingRotation.y, startingRotation.x, 0f);
            }
        }
    }
}
