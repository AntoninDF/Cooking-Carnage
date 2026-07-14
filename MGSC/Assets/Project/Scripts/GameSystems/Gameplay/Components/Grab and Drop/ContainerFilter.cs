using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Features;
using System;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class ContainerFilter : MonoBehaviour
{
    public enum Filter {HasFeature, HasNotFeature}
    [SerializeField] public List<Filter> Filters;
    [SerializeField] public List<SerializableSystemType> Types;

    void Start()
    {
        foreach (var comp in gameObject.GetComponents<Component>())
        {
            if (comp.TryGetFeature<F_Container>(out var target))
            {
                for (int i = 0; i < Filters.Count; i++)
                {
                    int index = i;
                    target.Filter.AddCondition(
                        obj => obj is Feature f && f.HasSibling(Types[index].SystemType) == (Filters[index] == Filter.HasFeature));
                }
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ContainerFilter))]
public class ContainerFilterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Undo.RecordObject(target, "Modify filter");
        DrawConditionList();
        DrawAddMenu();
    }
    void DrawAddMenu()
    {
        var data = (ContainerFilter)target;
        if (GUILayout.Button("Add condition"))
        {
            GetType(type => AddCondition(ContainerFilter.Filter.HasFeature, type));
        }
    }

    private void AddCondition(ContainerFilter.Filter filter, SerializableSystemType type)
    {
        var data = (ContainerFilter)target;
        data.Filters.Add(filter);
        data.Types.Add(type);
    }

    void DrawConditionList()
    {
        var data = (ContainerFilter)target;
        for (int i = 0; i < data.Filters.Count; i++)
        {
            DrawCondition(i);
        }
    }

    void DrawCondition(int index)
    {
        var data = (ContainerFilter)target;
        if (index >= data.Filters.Count) return;
        if (index >= data.Types.Count) return;

        GUILayout.BeginHorizontal();
        data.Filters[index] = (ContainerFilter.Filter)EditorGUILayout.EnumPopup(data.Filters[index]);
        if (GUILayout.Button(data.Types[index].Name))
        {
            GetType(type => data.Types[index] = type, data.Types[index]);
        }
        if (GUILayout.Button("Del"))
        {
            data.Filters.RemoveAt(index);
            data.Types.RemoveAt(index);
        }
        EditorGUILayout.EndHorizontal();
    }
    void GetType(Action<SerializableSystemType> onTypeSelection, SerializableSystemType current = null)
    {
        if (onTypeSelection == null) return;
        GenericMenu menu = new();
        foreach (var type in GetSerializableFeatureTypes())
        {
            menu.AddItem(new(type.Name), type == current, () => onTypeSelection(type));
        }
        menu.ShowAsContext();
    }
    SerializableSystemType[] GetSerializableFeatureTypes()
    {
        var features = GetFeatureTypes();
        SerializableSystemType[] result = new SerializableSystemType[features.Length];
        for (int i = 0; i < features.Length; i++)
            result[i] = new(features[i]);
        return result;
    }
    Type[] GetFeatureTypes()
    {
        List<Type> result = new();
        foreach (var type in Assembly.GetCallingAssembly().GetTypes())
        {
            if (!type.IsSubclassOf(typeof(Feature))) continue;
            result.Add(type);
        }
        return result.ToArray();
    }
}
#endif