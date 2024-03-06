using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EditorButton))]
public class EventButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorButton myScript = (EditorButton)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Execute Events"))
        {
            myScript.OnPress();
        }
    }
}