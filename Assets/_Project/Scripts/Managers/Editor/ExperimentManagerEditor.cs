using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ExperimentManager))]
public class ExperimentManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        ExperimentManager manager = (ExperimentManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Record Current Positions as Targets"))
        {
            RecordTargets(manager);
        }
        
        if (GUILayout.Button("Snap Objects to Targets"))
        {
            SnapToTargets(manager);
        }
    }

    private void RecordTargets(ExperimentManager manager)
    {
        Undo.RecordObject(manager, "Record Experiment Targets");
        
        for (int i = 0; i < manager.targets.Count; i++)
        {
            var target = manager.targets[i];
            if (target.interactableObject != null)
            {
                target.targetPosition = target.interactableObject.transform.position;
                target.targetRotation = target.interactableObject.transform.eulerAngles;
                manager.targets[i] = target; // Structs need to be re-assigned
                Debug.Log($"Recorded {target.interactableObject.name} at {target.targetPosition}");
            }
        }
        
        EditorUtility.SetDirty(manager);
    }

    private void SnapToTargets(ExperimentManager manager)
    {
        foreach (var target in manager.targets)
        {
            if (target.interactableObject != null)
            {
                Undo.RecordObject(target.interactableObject.transform, "Snap to Target");
                target.interactableObject.transform.position = target.targetPosition;
                target.interactableObject.transform.rotation = Quaternion.Euler(target.targetRotation);
            }
        }
    }
}
