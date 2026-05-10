using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;

public class QuizBoardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionText;

    [SerializeField] private GameObject answerButtonPrefab;
    [SerializeField] private Transform answerContainer;

    [SerializeField] private TextMeshProUGUI explainationText;

    private readonly List<QuizAnswerButtonItem> buttonPool = new List<QuizAnswerButtonItem>();

    public void ShowNoQuestion(string message)
    {
        SetQuestionText(message);
        SetExplainationText(string.Empty);
        HideAllAnswerButtons();
    }

    public void ShowCompleted(string message)
    {
        SetQuestionText(message);
        SetExplainationText(string.Empty);
        HideAllAnswerButtons();
    }

    public void SetQuestionText(string questionText)
    {
        this.questionText.text = questionText;
    }

    public void SetExplainationText(string explainationText)
    {
        this.explainationText.text = explainationText;
    }

    public void BindAnswers(IReadOnlyList<string> answers, Action<int> onAnswerSelected)
    {
        EnsurePoolSize(answers.Count);

        for (int i = 0; i < buttonPool.Count; i++)
        {
            bool active = i < answers.Count;
            buttonPool[i].gameObject.SetActive(active);

            if (!active) continue;

            int displayIndex = i;
            buttonPool[i].SetLabel(answers[i]);
            buttonPool[i].SetOnClick(() => onAnswerSelected?.Invoke(displayIndex));
        }
    }

    private void EnsurePoolSize(int requiredCount)
    {
        if (answerButtonPrefab == null || answerContainer == null) return;

        while (buttonPool.Count < requiredCount)
        {
            GameObject buttonObj = Instantiate(answerButtonPrefab, answerContainer);
            QuizAnswerButtonItem item = buttonObj.GetComponent<QuizAnswerButtonItem>();
            if (item == null)
            {
                item = buttonObj.AddComponent<QuizAnswerButtonItem>();
            }

            buttonPool.Add(item);
        }
    }

    private void HideAllAnswerButtons()
    {
        for (int i = 0; i < buttonPool.Count; i++)
        {
            buttonPool[i].gameObject.SetActive(false);
        }
    }

}
