// Source - https://stackoverflow.com/a/49301462
// Posted by High, modified by community. See post 'Timeline' for change history
// Retrieved 2026-03-05, License - CC BY-SA 3.0

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
public class DefaultMonoBehaviourEditor : Editor
{
    private bool hideScriptField;

    private void OnEnable()
    {
        hideScriptField = target.GetType().GetCustomAttributes(typeof(HideScriptFieldAttribute), false).Length > 0;
    }

    public override void OnInspectorGUI()
    {
        if (hideScriptField)
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
        else
        {
            base.OnInspectorGUI();
        }
    }
}
