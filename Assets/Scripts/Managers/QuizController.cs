using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class QuizController : MonoBehaviour
{
    public event Action QuizFinished;
    public event Action<QuizResult> LessonCompleted;

    [SerializeField] private QuizBoardUI quizBoardUI;

    [SerializeField] private float nextQuestionDelay = 1f;

    private readonly QuizModel model = new QuizModel();
    private readonly List<int> answerIndexMap = new List<int>();

    private bool waitingForNextQuestion;
    private Coroutine nextQuestionCoroutine;

    public void Load(string lessonKey)
    {
        List<QuestionData> questions = AssetManager.Ins != null
            ? AssetManager.Ins.GetQuestionsByLesson(lessonKey)
            : new List<QuestionData>();

        model.SetQuestions(questions);
        waitingForNextQuestion = false;

        if (nextQuestionCoroutine != null)
        {
            StopCoroutine(nextQuestionCoroutine);
            nextQuestionCoroutine = null;
        }

        if (!model.HasQuestions)
        {
            quizBoardUI.ShowNoQuestion("Không có câu hỏi.");
            return;
        }

        ShowCurrentQuestion();
    }

    public void ShowCompletionSummary(string message)
    {
        if (quizBoardUI == null) return;
        quizBoardUI.ShowCompleted(message);
    }

    private void ShowCurrentQuestion()
    {
        if (model.IsCompleted)
        {
            quizBoardUI.ShowCompleted("Hoàn thành bài học!");
            LessonCompleted?.Invoke(new QuizResult(model.CorrectCount, model.TotalQuestions));
            QuizFinished?.Invoke();
            return;
        }

        waitingForNextQuestion = false;
        quizBoardUI.SetExplainationText(string.Empty);

        QuestionData question = model.CurrentQuestion;
        List<string> answerLabels = BuildAnswerLabels(question);

        quizBoardUI.SetQuestionText(question.questionText);
        quizBoardUI.BindAnswers(answerLabels, OnAnswerSelected);
    }

    private string GetChoicePrefix(int index)
    {
        return ((char)('A' + index)).ToString();
    }

    private List<string> BuildAnswerLabels(QuestionData question)
    {
        answerIndexMap.Clear();
        List<string> labels = new List<string>();

        if (question == null || question.answers == null)
        {
            return labels;
        }

        for (int i = 0; i < question.answers.Length; i++)
        {
            string answerText = question.answers[i];
            if (string.IsNullOrWhiteSpace(answerText)) continue;

            string choicePrefix = GetChoicePrefix(labels.Count);
            labels.Add($"{choicePrefix}.   {answerText}");
            answerIndexMap.Add(i);
        }

        return labels;
    }

    private void OnAnswerSelected(int displayIndex)
    {
        if (waitingForNextQuestion) return;
        if (displayIndex < 0 || displayIndex >= answerIndexMap.Count) return;

        int selectedIndex = answerIndexMap[displayIndex];
        QuestionData question = model.CurrentQuestion;
        bool isCorrect = selectedIndex == question.correctIndex;
        model.RegisterAnswer(isCorrect);

        quizBoardUI.SetExplainationText((isCorrect ? "Right!\n" : "Wrong!\n") + question.explanation);

        waitingForNextQuestion = true;
        nextQuestionCoroutine = StartCoroutine(LoadNextQuestionAfterDelay());
    }

    public readonly struct QuizResult
    {
        public int CorrectCount { get; }
        public int TotalQuestions { get; }

        public QuizResult(int correctCount, int totalQuestions)
        {
            CorrectCount = correctCount;
            TotalQuestions = totalQuestions;
        }
    }

    private IEnumerator LoadNextQuestionAfterDelay()
    {
        yield return new WaitForSeconds(nextQuestionDelay);
        model.MoveNext();
        nextQuestionCoroutine = null;
        ShowCurrentQuestion();
    }

    private void OnDisable()
    {
        if (nextQuestionCoroutine != null)
        {
            StopCoroutine(nextQuestionCoroutine);
            nextQuestionCoroutine = null;
        }
    }
}
