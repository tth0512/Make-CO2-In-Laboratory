using UnityEngine;
using UnityEngine.InputSystem;

public class QuizBoardInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private QuizController quizController;
    [SerializeField] private Camera quizCamera;
    [SerializeField] private string lessonKey = "Lesson 2";
    [SerializeField] private bool allowExitWithEscape = true;

    private PlayerController playerController;
    private Camera playerCamera;
    private bool inQuizMode;

    public void Interact(PlayerPickUp interactor)
    {
        if (inQuizMode || interactor == null || quizController == null || quizCamera == null) return;

        playerController = interactor.GetComponent<PlayerController>();
        playerCamera = playerController != null ? playerController.playerCamera : Camera.main;

        EnterQuizMode();
    }

    private void EnterQuizMode()
    {
        inQuizMode = true;

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (playerCamera != null)
        {
            playerCamera.enabled = false;
        }

        quizCamera.enabled = true;

        if (InteractionManager.Ins != null)
        {
            InteractionManager.Ins.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        quizController.Load(lessonKey);
    }

    private void Update()
    {
        if (!inQuizMode || !allowExitWithEscape) return;
        if (Keyboard.current == null) return;
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        ExitQuizMode();
    }

    private void ExitQuizMode()
    {
        if (!inQuizMode) return;

        inQuizMode = false;

        if (quizCamera != null)
        {
            quizCamera.enabled = false;
        }

        if (playerCamera != null)
        {
            playerCamera.enabled = true;
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (InteractionManager.Ins != null)
        {
            InteractionManager.Ins.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        ExitQuizMode();
    }
}
