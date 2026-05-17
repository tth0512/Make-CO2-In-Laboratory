using UnityEditor;

namespace DarkMoorGames.ChannelPackerPro
{
    [CustomEditor(typeof(FlipPostprocessor))]
    public sealed class FlipPostprocessorEditor : Editor
    {
        SerializedProperty flipHorizontal;
        SerializedProperty flipVertical;

        void OnEnable()
        {
            if (target == null)
                return;
            flipHorizontal = serializedObject.FindProperty("flipHorizontal");
            flipVertical = serializedObject.FindProperty("flipVertical");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(flipHorizontal);
            EditorGUILayout.PropertyField(flipVertical);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
