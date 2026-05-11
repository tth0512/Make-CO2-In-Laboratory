using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    public sealed class MeshSelectWindow : EditorWindow
    {
        EditorWindow parent;
        PackingGroup group;
        ImageSlot slot;

        public static void ShowMeshSelectAuxWindow(PackingGroup group, ImageSlot slot, EditorWindow parent)
        {
            MeshSelectWindow window = CreateInstance<MeshSelectWindow>();
            window.hideFlags = HideFlags.DontSave;
            window.titleContent = new GUIContent("Select Mesh");
            window.parent = parent;
            window.group = group;
            window.slot = slot;
            window.maxSize = new Vector2(250f, 50f);
            window.minSize = window.maxSize;
            window.ShowAuxWindow();
        }
        void OnGUI()
        {
            float width = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 70f;
            EditorGUI.BeginChangeCheck();
            slot.UvDisplay.mesh = EditorGUILayout.ObjectField(new GUIContent("Mesh"), slot.UvDisplay.mesh, typeof(Mesh), false) as Mesh;
            if (EditorGUI.EndChangeCheck())
            {
                slot.UvDisplay.CacheMeshData();
                group.FindAndTryPostprocessImage(slot);
                if (parent)
                    parent.Repaint();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Update Uvs"))
            {
                slot.UvDisplay.CacheMeshData();
                group.FindAndTryPostprocessImage(slot);
                if (parent)
                    parent.Repaint();
            }
            EditorGUIUtility.labelWidth = width;
        }
    }
}
