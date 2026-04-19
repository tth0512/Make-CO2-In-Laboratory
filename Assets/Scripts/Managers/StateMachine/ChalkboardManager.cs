using UnityEngine;
using Cinemachine;

public class ChalkboardManager : MonoBehaviour, IInteractable
{
    [Header("References")]
    public CinemachineVirtualCamera chalkboardCam; // Camera quay cận cảnh bảng
    public GameObject playerController;            // Kéo script Player vào đây
    public GameObject quizCanvas;                  // UI Canvas chứa câu hỏi
    public QuizController quizController;
    public string lessonKey = "Lesson 2";

    // Cỗ máy StateMachine siêu mượt của chúng ta
    public StateMachine<BaseChalkboardState> stateMachine { get; private set; }
    public int lastSelectedAnswer;

    void Awake()
    {
        stateMachine = new StateMachine<BaseChalkboardState>();

        // Nạp các State
        stateMachine.AddState(new ChalkboardInactiveState(this));
        stateMachine.AddState(new ChalkboardQuestionState(this));
        // stateMachine.AddState(new ChalkboardFeedbackState(this));...

        if (quizController != null)
        {
            quizController.QuizFinished += HandleQuizFinished;
        }
    }

    void Start()
    {
        // Mặc định vào game, bảng ở trạng thái Ngủ đông
        stateMachine.Initialize<ChalkboardInactiveState>();
    }

    void Update()
    {
        stateMachine.Update();
    }

    // ==========================================
    // HÀM NÀY CHẠY KHI NGƯỜI CHƠI NHÌN VÀO BẢNG VÀ ẤN 'E'
    // ==========================================
    public void Interact(PlayerPickUp interactor)
    {
        // Chỉ cho phép tương tác nếu bảng đang Ngủ đông
        if (stateMachine.CurrentState is ChalkboardInactiveState)
        {
            stateMachine.ChangeState<ChalkboardQuestionState>();
        }
    }


    // Hàm nhận sự kiện click từ UI Button (A, B, C, D)
    public void SubmitAnswer(int index)
    {
        stateMachine.CurrentState?.HandleAnswerClick(index);
    }

    public void EnterQuestionMode()
    {
        if (quizCanvas != null)
        {
            quizCanvas.SetActive(true);
        }

        if (chalkboardCam != null)
        {
            chalkboardCam.Priority = 20;
        }

        SetPlayerControl(false);
        SetGameplayInteraction(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (quizController != null)
        {
            quizController.Load(lessonKey);
        }
    }

    public void ExitQuestionMode()
    {
        if (quizCanvas != null)
        {
            quizCanvas.SetActive(false);
        }

        if (chalkboardCam != null)
        {
            chalkboardCam.Priority = 0;
        }

        SetPlayerControl(true);
        SetGameplayInteraction(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetPlayerControl(bool isEnabled)
    {
        if (playerController == null) return;

        PlayerController controller = playerController.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = isEnabled;
        }
    }

    private void SetGameplayInteraction(bool isEnabled)
    {
        if (InteractionManager.Ins != null)
        {
            InteractionManager.Ins.enabled = isEnabled;
        }
    }

    private void HandleQuizFinished()
    {
        if (stateMachine.CurrentState is ChalkboardQuestionState)
        {
            stateMachine.ChangeState<ChalkboardInactiveState>();
        }
    }

    private void OnDestroy()
    {
        if (quizController != null)
        {
            quizController.QuizFinished -= HandleQuizFinished;
        }
    }

    
}