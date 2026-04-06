using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuizController : MonoBehaviour
{
    public List<QuizSet> quizSets;

    public GameObject quizPanel;
    public Text questionText;
    public Button[] answerButtons;
    public Text explanationText;

    private List<Question> currentQuestions;
    private int currentIndex = 0;
    private int currentLesson;

    public void StartQuiz(int lessonId)
    {
        quizPanel.SetActive(true);
        currentLesson = lessonId;

        foreach (var set in quizSets)
        {
            if (set.lessonId == lessonId)
            {
                currentQuestions = set.questions;
                break;
            }
        }

        currentIndex = 0;
        ShowQuestion();
    }

    void ShowQuestion()
    {
        var q = currentQuestions[currentIndex];

        questionText.text = q.questionText;

        for (int i = 0; i < 4; i++)
        {
            int index = i;
            answerButtons[i].GetComponentInChildren<Text>().text = q.answers[i];

            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => CheckAnswer(index));
        }

        explanationText.text = "";
    }

    void CheckAnswer(int index)
    {
        var q = currentQuestions[currentIndex];

        if (index == q.correctIndex)
            explanationText.text = "Right!\n" + q.explanation;
        else
            explanationText.text = "Wrong!\n" + q.explanation;
    }

    public void CloseQuiz()
    {
        quizPanel.SetActive(false);
    }
}
