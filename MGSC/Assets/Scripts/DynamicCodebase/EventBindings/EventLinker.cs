
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public partial class EventLinker : MonoBehaviour
{
    public List<string> displayedMembers;
    //public List<string> options;

    public void TryAdd(string member)
    {
        if (displayedMembers == null) return;
        if (displayedMembers.Contains(member))
            return;
        displayedMembers.Add(member);
    }
    public void Clear() => displayedMembers = new();
    public void Remove(string member)
    {
        if (displayedMembers == null) return;
        if (!displayedMembers.Contains(member)) return;
        displayedMembers.Remove(member);
    }
}

[CustomEditor(typeof(EventLinker))]
public class EventLinkerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        
        var data = (EventLinker)target;

        if (data.displayedMembers == null)
        {
            data.displayedMembers = new();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add event"))
        {
            GenericMenu menu = new GenericMenu();
            foreach(string option in data.options)
            {
                menu.AddItem(new GUIContent(option), false, () => data.TryAdd(option));
            }

            menu.ShowAsContext();
        }
        if (GUILayout.Button("Clear all"))
        {
            data.Clear();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        foreach(var name in data.displayedMembers)
        {
            var prop = serializedObject.FindProperty(name);
            if (prop != null)
            {
                if (GUILayout.Button("Remove " + name))
                {
                    data.Remove(name);
                    break;
                }
                EditorGUILayout.PropertyField(prop);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}