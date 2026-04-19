using System.Collections.Generic;

public class QuizModel
{
    private List<QuestionData> questions = new List<QuestionData>();
    private int currentIndex;

    public bool HasQuestions => questions != null && questions.Count > 0;
    public bool IsCompleted => !HasQuestions || currentIndex >= questions.Count;

    public QuestionData CurrentQuestion
    {
        get
        {
            if (IsCompleted) return null;
            return questions[currentIndex];
        }
    }

    public void SetQuestions(List<QuestionData> source)
    {
        questions = source ?? new List<QuestionData>();
        currentIndex = 0;
    }

    public void MoveNext()
    {
        if (!IsCompleted)
        {
            currentIndex++;
        }
    }
}
