using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class QuizController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionText;

    [SerializeField] private GameObject answerButtonPrefab;
    [SerializeField] private Transform answerContainer;

    [SerializeField] private TextMeshProUGUI explainationText;

    [SerializeField] private float nextQuestionDelay = 0.75f;

    private List<QuestionData> currentQuestions = new List<QuestionData>();
    private int currentQuestionIndex;
    private bool waitingForNextQuestion;

    private void Awake()
    {
        //questionText = transform.Find("QuestionText").GetComponent<TextMeshPro>();
        //answerButtonPrefab = Resources.Load<GameObject>("AnswerButton");
        //answerContainer = transform.Find("AnswerContainer");
        //explainationText = transform.Find("ExplainationText").GetComponent<TextMeshPro>();
    }

    private void Start()
    {
        Load("Lession 2");
    }

    public void Load(string lessonKey)
    {
        currentQuestions = AssetManager.Ins.GetQuestionsByLesson(lessonKey);
        currentQuestionIndex = 0;
        waitingForNextQuestion = false;

        if (currentQuestions == null || currentQuestions.Count == 0)
        {
            questionText.text = "Không có câu hỏi.";
            explainationText.text = string.Empty;
            ClearAnswerButtons();
            return;
        }

        ShowCurrentQuestion();
    }

    private void ShowCurrentQuestion()
    {
        if (currentQuestionIndex >= currentQuestions.Count)
        {
            questionText.text = "Hoàn thành bài học!";
            explainationText.text = string.Empty;
            ClearAnswerButtons();
            return;
        }

        waitingForNextQuestion = false;
        explainationText.text = string.Empty;
        ClearAnswerButtons();

        QuestionData question = currentQuestions[currentQuestionIndex];
        questionText.text = question.questionText;

        for (int i = 0; i < question.answers.Length; i++)
        {
            int answerIndex = i;
            var buttonObj = Instantiate(answerButtonPrefab, answerContainer);
            var buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            string choicePrefix = GetChoicePrefix(i);
            buttonText.text = $"{choicePrefix}.   {question.answers[i]}";

            var button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => OnAnswerSelected(answerIndex));
        }
    }

    private string GetChoicePrefix(int index)
    {
        return ((char)('A' + index)).ToString();
    }

    private void OnAnswerSelected(int selectedIndex)
    {
        if (waitingForNextQuestion) return;

        QuestionData question = currentQuestions[currentQuestionIndex];
        bool isCorrect = selectedIndex == question.correctIndex;

        explainationText.text = (isCorrect ? "Right!\n" : "Wrong!\n") + question.explanation;

        waitingForNextQuestion = true;
        StartCoroutine(LoadNextQuestionAfterDelay());
    }

    private IEnumerator LoadNextQuestionAfterDelay()
    {
        yield return new WaitForSeconds(nextQuestionDelay);
        currentQuestionIndex++;
        ShowCurrentQuestion();
    }

    private void ClearAnswerButtons()
    {
        if (answerContainer == null) return;

        for (int i = answerContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(answerContainer.GetChild(i).gameObject);
        }
    }
}
