
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

// REFACTO : Support references to individual objects, to retrieve their specific Actions without making a global scan.

public partial class EventLinker : MonoBehaviour
{
    
    public List<string> displayedMembers;
    public static Action<object> OnRebind;
    //public List<string> options;

    private void Awake()
    {
        OnRebind += BindEvents;
    }

    public void Debug(string msg)
    {
        UnityEngine.Debug.Log(msg);
    }

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

    public static void Bind(object obj)
    {
        OnRebind?.Invoke(obj);
    }
    public void BindEvents(object obj)
    {
        foreach (var member in obj.GetType().GetMembers())
        {
            BindEventAttribute attr = member.GetCustomAttribute<BindEventAttribute>();
            if (attr == null)
                continue;
            if (member.MemberType != MemberTypes.Field)
                continue;
            FieldInfo field = (FieldInfo)member;
            foreach(string actionName in displayedMembers)
            {
                if (attr.Name == actionName)
                {
                    Action action = field.GetValue(obj) as Action;
                    //if (action == null) continue;
                    string parsedName = actionName.Split("/")[^1];
                    foreach (FieldInfo myField in typeof(EventLinker).GetFields())
                    {
                        if (myField.Name == parsedName && myField.FieldType == typeof(UnityEvent))
                        {
                            UnityEvent myEvent = myField.GetValue(this) as UnityEvent;
                            if (myEvent == null) continue;
                            action += myEvent.Invoke;
                        }
                    }
                    field.SetValue(obj, action);
                }
            }
        }
        
    }

}

#if UNITY_EDITOR

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
            if (data.options.Count == 0)
                menu.AddItem(new GUIContent("No Actions available"), false, () => data.TryAdd(""));

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
            string parsedName = name.Split("/")[^1];
            var prop = serializedObject.FindProperty(parsedName);
            if (prop != null)
            {
                if (GUILayout.Button("Remove " + parsedName))
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

#endif