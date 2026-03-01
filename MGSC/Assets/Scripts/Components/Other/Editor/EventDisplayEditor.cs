using FMODUnity;
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

[CustomEditor(typeof(EventDisplay))]
public class EventDisplayEditor : Editor
{    
    public override void OnInspectorGUI()
    {
        EventDisplay display = (EventDisplay)target;

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add event"))
        {
            GenericMenu menu = new GenericMenu();
            var fields = display.GetAvailableActions();
            //Debug.Log(fields.Count + " fields found.");
            foreach ( var action in fields )
            {
                string name = action.Item1 + "/" + action.actionInfo.Name;
                //Debug.Log(action.Item1 + " " + action.Item2 + " " +  action.Item3);
                var wrapper = new UnityEventWrapper(
                    name,
                    new UnityEngine.Events.UnityEvent(),
                    action.actionInfo.Name, 
                    action.featureInfo.Name, 
                    action.actionInfo.DeclaringType, 
                    action.featureInfo.DeclaringType
                    );
                menu.AddItem(new GUIContent(name), false, () => display.AddEvent(wrapper));
            }
            menu.ShowAsContext();
        }
        if (GUILayout.Button("Remove all events"))
        {
            display.Events.Clear();
        }

        EditorGUILayout.EndHorizontal();

        ListIterator("Events");

        EditorGUI.EndChangeCheck();
        serializedObject.ApplyModifiedProperties();
    }

    

    public void ListIterator(string eventList)
    {

        EventDisplay display = (EventDisplay)target;
        SerializedProperty listProperty = serializedObject.FindProperty(eventList);

        for (int i = 0; i < listProperty.arraySize; i++)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(i);
            var wrapper = (UnityEventWrapper)elementProperty.boxedValue;
            SerializedProperty serializedEvent = elementProperty.FindPropertyRelative("Event");

            string propertyName = wrapper.name.Replace("/", " -> ");

            GUILayout.BeginHorizontal();

            if (display.IsValid(wrapper))
            {
                EditorGUILayout.PropertyField(serializedEvent,new GUIContent(propertyName));
            }
            else
            {
                Color previous = GUI.color;
                GUI.color = new Color(1f, 0.5f, 0.5f);

                EditorGUILayout.PropertyField(serializedEvent, new GUIContent(propertyName));
                GUI.color = previous;
            }

                GUILayout.BeginVertical(GUILayout.MaxWidth(60));
            if (GUILayout.Button("Remove", GUILayout.Height(EditorGUI.GetPropertyHeight(serializedEvent)/2)))
            {
                display.Events.RemoveAt(i);
            }
            if (GUILayout.Button("Clear", GUILayout.Height(EditorGUI.GetPropertyHeight(serializedEvent) / 2)))
            {
                ClearEvent(i);
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        void ClearEvent(int index)
        {
            var e = display.Events[index];

            var wrapper = new UnityEventWrapper(
                e.name,
                new UnityEngine.Events.UnityEvent(),
                e.actionMemberInfoName,
                e.featureMemberInfoName,
                e.featureType.SystemType,
                e.componentType.SystemType);

            display.Events[index] = wrapper;
        }
    }
}
