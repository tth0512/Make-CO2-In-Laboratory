using UnityEditor;

namespace DarkMoorGames.ChannelPackerPro
{
    [CustomEditor(typeof(BlurPostprocessor))]
    public sealed class BlurPostprocessorEditor : Editor
    {
        SerializedProperty iterations;
        SerializedProperty clamp;

        void OnEnable()
        {
            if (target == null)
                return;
            iterations = serializedObject.FindProperty("iterations");
            clamp = serializedObject.FindProperty("clamp");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.IntSlider(iterations, 1, 16);
            EditorGUILayout.PropertyField(clamp);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
