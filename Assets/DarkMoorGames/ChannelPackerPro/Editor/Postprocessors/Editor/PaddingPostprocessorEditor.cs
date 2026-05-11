using UnityEditor;

namespace DarkMoorGames.ChannelPackerPro
{
    [CustomEditor(typeof(PaddingPostprocessor))]
    public sealed class PaddingPostprocessorEditor : Editor
    {
        SerializedProperty method;
        SerializedProperty infinite;
        SerializedProperty padding;
        SerializedProperty clamp;

        void OnEnable()
        {
            if (target == null)
                return;
            method = serializedObject.FindProperty("method");
            infinite = serializedObject.FindProperty("infinite");
            padding = serializedObject.FindProperty("padding");
            clamp = serializedObject.FindProperty("clamp");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(method);
            EditorGUILayout.PropertyField(infinite);
            EditorGUILayout.PropertyField(clamp);

            EditorGUI.BeginDisabledGroup(infinite.boolValue);
            EditorGUILayout.IntSlider(padding, 1, 256);
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
