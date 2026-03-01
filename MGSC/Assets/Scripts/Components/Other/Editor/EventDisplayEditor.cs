using FMODUnity;
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

[CustomEditor(typeof(EventDisplay))]
public class EventDisplayEditor : Editor
{
    bool listVisibility = false;
    
    public override void OnInspectorGUI()
    {
        EventDisplay display = (EventDisplay)target;

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add event"))
        {
            GenericMenu menu = new GenericMenu();
            var fields = display.GetAvailableActions();
            Debug.Log(fields.Count + " fields found.");
            foreach ( var action in fields )
            {
                var actionMember = action.Item2;
                string name = action.Item1 + "/" + actionMember.Name;
                Debug.Log(action.Item1 + " " + action.Item2 + " " +  action.Item3);
                menu.AddItem(new GUIContent(name), false, () => display.AddEvent(actionMember, action.Item3, name));
            }
            menu.ShowAsContext();
        }
        if (GUILayout.Button("Clear events"))
        {
            display.Events.Clear();
        }

        EditorGUILayout.EndHorizontal();

        ListIterator("Events", "EventNames", "EventInfos");

        EditorGUI.EndChangeCheck();
        serializedObject.ApplyModifiedProperties();
    }

    

    public void ListIterator(string eventList, string eventNames, string eventInfos)
    {
        SerializedProperty listProperty = serializedObject.FindProperty(eventList);
        SerializedProperty names = serializedObject.FindProperty(eventNames);
        SerializedProperty infos = serializedObject.FindProperty(eventInfos);

        for (int i = 0; i < listProperty.arraySize; i++)
        {
            SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(i);
            var wrapper = (UnityEventWrapper)elementProperty.boxedValue;
            SerializedProperty serializedEvent = elementProperty.FindPropertyRelative("Event");
            bool showChildren = EditorGUILayout.PropertyField(serializedEvent,new GUIContent(wrapper.name.Replace("/", " -> ")));
        }
        
    }
}
