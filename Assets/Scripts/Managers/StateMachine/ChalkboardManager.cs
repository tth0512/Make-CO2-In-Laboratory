using UnityEngine;
using Cinemachine;
using System.Collections.Generic;
using System.Collections;
[DefaultExecutionOrder(0)]

public class ChalkboardManager : MonoBehaviour, IInteractable
{
    [Header("References")]
    public CinemachineVirtualCamera chalkboardCam; // Camera quay cận cảnh bảng
    public GameObject playerController;            // Kéo script Player vào đây
    public GameObject quizCanvas;                  // UI Canvas chứa câu hỏi
    public QuizController quizController;

    [Header("Lessons")]
    public bool loopLessons;
    [SerializeField] private float nextLessonDelay = 3f;
    private List<string> lessons = new List<string>();
    private int currentLessonIndex;
    private bool pendingNextLesson;

    private PlayerManager playerManager;
    // Cỗ máy StateMachine siêu mượt của chúng ta
    public StateMachine<BaseChalkboardState> stateMachine { get; private set; }
    public int lastSelectedAnswer;

    void Awake()
    {
        playerManager = PlayerManager.Instance;
        stateMachine = new StateMachine<BaseChalkboardState>();

        RefreshLessonsFromAssets();

        // Nạp các State
        stateMachine.AddState(new ChalkboardInactiveState(this));
        stateMachine.AddState(new ChalkboardQuestionState(this));
        // stateMachine.AddState(new ChalkboardFeedbackState(this));...

        if (quizController != null)
        {
            quizController.QuizFinished += HandleQuizFinished;
            quizController.LessonCompleted += HandleLessonCompleted;
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
        if (lessons == null || lessons.Count == 0)
        {
            RefreshLessonsFromAssets();
        }

        if (lessons == null || lessons.Count == 0)
        {
            if (quizCanvas != null)
                quizCanvas.SetActive(true);
            if (quizController != null)
                quizController.Load(string.Empty);
            return;
        }

        string lessonKey = lessons[Mathf.Clamp(currentLessonIndex, 0, lessons.Count - 1)];
        if (quizCanvas != null)
        {
            quizCanvas.SetActive(true);
        }

        if (chalkboardCam != null)
        {
            chalkboardCam.Priority = 20;
        }

        playerManager.DisablePlayer();

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

        if (playerManager != null)
            playerManager.EnablePlayer();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    

    private void HandleQuizFinished()
    {
        if (stateMachine.CurrentState is ChalkboardQuestionState)
        {
            if (pendingNextLesson)
                return;

            stateMachine.ChangeState<ChalkboardInactiveState>();
        }
    }

    private void HandleLessonCompleted(QuizController.QuizResult result)
    {
        string lessonName = GetCurrentLessonName();
        if (quizController != null)
        {
            quizController.ShowCompletionSummary($"Hoàn thành {lessonName}!\nKết quả: {result.CorrectCount}/{result.TotalQuestions}");
        }

        bool hasNextLesson = HasNextLesson();
        AdvanceLesson();

        if (hasNextLesson)
        {
            pendingNextLesson = true;
            StartCoroutine(LoadNextLessonAfterDelay());
        }
    }

    private IEnumerator LoadNextLessonAfterDelay()
    {
        yield return new WaitForSeconds(nextLessonDelay);
        pendingNextLesson = false;

        if (stateMachine.CurrentState is ChalkboardQuestionState && quizController != null)
        {
            quizController.Load(GetCurrentLessonKey());
        }
    }

    private void AdvanceLesson()
    {
        if (lessons == null || lessons.Count == 0)
            return;

        currentLessonIndex++;
        if (currentLessonIndex >= lessons.Count)
        {
            currentLessonIndex = loopLessons ? 0 : lessons.Count - 1;
        }
    }

    private string GetCurrentLessonName()
    {
        if (lessons == null || lessons.Count == 0)
            return "bài học";

        return lessons[Mathf.Clamp(currentLessonIndex, 0, lessons.Count - 1)];
    }

    private string GetCurrentLessonKey()
    {
        if (lessons == null || lessons.Count == 0)
            return string.Empty;

        return lessons[Mathf.Clamp(currentLessonIndex, 0, lessons.Count - 1)];
    }

    private bool HasNextLesson()
    {
        if (lessons == null || lessons.Count == 0)
            return false;

        return loopLessons || currentLessonIndex < lessons.Count - 1;
    }

    private void RefreshLessonsFromAssets()
    {
        lessons.Clear();
        if (AssetManager.Ins == null)
            return;

        List<string> detectedLessons = AssetManager.Ins.GetLessonKeys();
        detectedLessons.Sort();
        lessons.AddRange(detectedLessons);
        currentLessonIndex = Mathf.Clamp(currentLessonIndex, 0, Mathf.Max(0, lessons.Count - 1));
    }

    private void OnDestroy()
    {
        if (quizController != null)
        {
            quizController.QuizFinished -= HandleQuizFinished;
            quizController.LessonCompleted -= HandleLessonCompleted;
        }
    }

    
}