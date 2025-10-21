using UnityEngine;
using UnityEditor;
using Cooking;

[CustomEditor(typeof(ContainerFilter))]
[CanEditMultipleObjects]
public class ContainerFilterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        serializedObject.Update();
        ContainerFilter filter = target as ContainerFilter;
        if (filter == null) return;

        var myEnumProp = serializedObject.FindProperty("filterOperator");
        EditorGUILayout.PropertyField(myEnumProp);
        filter.item = GUILayout.Toggle(filter.item, "Item");
        if (filter.item)
        {
            filter.ingredient = GUILayout.Toggle(filter.ingredient, "Ingredient");
            filter.plate = GUILayout.Toggle(filter.plate, "Plate");
            filter.meal = GUILayout.Toggle(filter.meal, "Meal");

        }
        filter.container = GUILayout.Toggle(filter.container, "Container");

        serializedObject.ApplyModifiedProperties();
    }
}
