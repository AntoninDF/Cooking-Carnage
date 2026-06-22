using Localization;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizedText))]
public class LocalizedTextEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var data = (LocalizedText)target;


        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh text"))
        {
            data.UpdateText();
        }
        if (GUILayout.Button("Refresh localization data"))
        {
            if (data.Database != null)
                data.Database.Load();
        }
        GUILayout.EndHorizontal();
    }
}
