using UnityEditor;

namespace DarkMoorGames.ChannelPackerPro
{
    [CustomEditor(typeof(OffsetPostprocessor))]
    public sealed class OffsetPostprocessorEditor : Editor
    {
        SerializedProperty offset;

        void OnEnable()
        {
            if (target == null)
                return;
            offset = serializedObject.FindProperty("offset");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(offset);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
