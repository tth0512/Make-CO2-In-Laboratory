using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    [CustomEditor(typeof(ColorReplacePostprocessor))]
    public class ColorReplacePostprocessorEditor : Editor
    {
        SerializedProperty min;
        SerializedProperty max;
        SerializedProperty from;
        SerializedProperty to;

        GUIStyle label;

        private void OnEnable()
        {
            if (target == null)
                return;
            min = serializedObject.FindProperty("min");
            max = serializedObject.FindProperty("max");
            from = serializedObject.FindProperty("from");
            to = serializedObject.FindProperty("to");
        }
        public override void OnInspectorGUI()
        {
            if (label == null)
            {
                label = new GUIStyle(EditorStyles.miniBoldLabel)
                {
                    imagePosition = ImagePosition.TextOnly,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    margin = new RectOffset(0, 0, 0, 0)
                };
            }
            serializedObject.UpdateIfRequiredOrScript();

            float minValue = min.floatValue;
            float maxValue = max.floatValue;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.MinMaxSlider("Range", ref minValue, ref maxValue, 0f, 1f);
            EditorGUILayout.LabelField("[" + minValue.ToString("F1") + "," + maxValue.ToString("F1") + "]", label, GUILayout.Width(65f));
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                min.floatValue = minValue;
                max.floatValue = maxValue;
            }
            from.colorValue = EditorGUILayout.ColorField(new GUIContent(from.displayName, from.tooltip), from.colorValue, true, false, false);
            to.colorValue = EditorGUILayout.ColorField(new GUIContent(to.displayName, to.tooltip), to.colorValue, true, false, false);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
