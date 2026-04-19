using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "QuizSet", menuName = "Quiz/Quiz Set")]
public class QuizSet : ScriptableObject
{
    public int lessonId;
    public List<QuestionData> questions;
}
