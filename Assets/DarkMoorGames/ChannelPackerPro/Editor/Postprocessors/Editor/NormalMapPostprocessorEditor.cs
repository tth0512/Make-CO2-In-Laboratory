using UnityEditor;

namespace DarkMoorGames.ChannelPackerPro
{
    [CustomEditor(typeof(NormalMapPostprocessor))]
    public sealed class NormalMapPostprocessorEditor : Editor
    {
        SerializedProperty strength;
        SerializedProperty clamp;

        void OnEnable()
        {
            if (target == null)
                return;
            strength = serializedObject.FindProperty("strength");
            clamp = serializedObject.FindProperty("clamp");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.Slider(strength, -100f, 100f);
            EditorGUILayout.PropertyField(clamp);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
