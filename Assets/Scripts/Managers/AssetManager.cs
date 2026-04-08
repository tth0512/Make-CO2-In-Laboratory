using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AssetManager : MonoBehaviour 
{
    public static AssetManager Ins { get; set; }

    [Serializable]
    private class QuizzInLessons
    {
        public string key;
        public QuestionData question;
    }

    [Header("Question Data Cache")]
    [SerializeField] private string editorQuestionRootFolder = "Assets/Data/Questions";
    [SerializeField] private List<QuizzInLessons> questionCache = new List<QuizzInLessons>();

    private readonly Dictionary<string, QuestionData> questionsByKey = new Dictionary<string, QuestionData>();

    public Dictionary<string, QuestionData> QuestionsByKey => questionsByKey;

    private void Awake()
    {
        if (Ins != null && Ins != this)
        {
            Destroy(this);
        }
        else
        {
            Ins = this;
        }

        BuildQuestionDictionary();
    }

    private void BuildQuestionDictionary()
    {
        questionsByKey.Clear();

        for (int i = 0; i < questionCache.Count; i++)
        {
            QuizzInLessons entry = questionCache[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.key) || entry.question == null) continue;

            if (!questionsByKey.ContainsKey(entry.key))
            {
                questionsByKey.Add(entry.key, entry.question);
            }
        }
    }

    public List<QuestionData> GetQuestionsByLesson(string lessonKey)
    {
        List<QuestionData> result = new List<QuestionData>();
        if (string.IsNullOrWhiteSpace(lessonKey)) return result;

        string prefix = lessonKey + "/";
        foreach (var kv in questionsByKey)
        {
            if (kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(kv.Value);
            }
        }

        return result;
    }

#if UNITY_EDITOR
    [ContextMenu("Refresh Question Cache From Folder")]
    private void RefreshQuestionCacheFromFolder()
    {
        questionCache.Clear();

        string[] guids = AssetDatabase.FindAssets("t:QuestionData", new[] { editorQuestionRootFolder });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            QuestionData question = AssetDatabase.LoadAssetAtPath<QuestionData>(path);
            if (question == null) continue;

            string lessonKey = GetLessonKeyFromPath(path);
            string questionId = string.IsNullOrWhiteSpace(question.questionID) ? question.name : question.questionID;
            string key = lessonKey + "/" + questionId;

            questionCache.Add(new QuizzInLessons
            {
                key = key,
                question = question
            });
        }

        BuildQuestionDictionary();
        EditorUtility.SetDirty(this);
    }

    private string GetLessonKeyFromPath(string assetPath)
    {
        if (string.IsNullOrWhiteSpace(assetPath)) return "UnknownLesson";
        if (string.IsNullOrWhiteSpace(editorQuestionRootFolder)) return "UnknownLesson";
        if (!assetPath.StartsWith(editorQuestionRootFolder, StringComparison.OrdinalIgnoreCase)) return "UnknownLesson";

        string relative = assetPath.Substring(editorQuestionRootFolder.Length).TrimStart('/');
        if (string.IsNullOrWhiteSpace(relative)) return "UnknownLesson";

        int slashIndex = relative.IndexOf('/');
        if (slashIndex <= 0) return "UnknownLesson";

        return relative.Substring(0, slashIndex);
    }
#endif
}


