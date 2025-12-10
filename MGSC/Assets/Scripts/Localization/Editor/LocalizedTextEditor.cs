using Localization;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizedText))]
public class LocalizedTextEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Refresh text"))
        {

        }
        if (GUILayout.Button("Refresh localization data"))
        {

        }
    }
}
