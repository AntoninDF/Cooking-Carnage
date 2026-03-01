using Features;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class EventDisplay : MonoBehaviour
{
    [SerializeField] public List<UnityEventWrapper> Events = new();



    void Start()
    {
        BindEvents();
    }

    private void BindEvents()
    {
        var hierarchy = GetChildren(gameObject);
        var components = GetComponentsFromGameObjects(hierarchy);
        foreach (var e in Events)
        {
            string[] parsedName = e.name.Split('/', '.');
            if (parsedName.Length != 4) continue;
            string goName = parsedName[0];
            string compName = parsedName[1];
            string featureName = parsedName[2];
            string actionName = parsedName[3];


            var actionInfo = new PropertyOrFieldInfo(e.actionMemberInfo);
            var featureInfo = new PropertyOrFieldInfo(e.featureMemberInfo);

            Debug.Log("Event okay : " + goName + " -> " + featureName + " -> " + actionName);

            var info = new PropertyOrFieldInfo(e.actionMemberInfo);
            Type actionType = info.Type;
            Debug.Log("Action type is " + actionType);
            Debug.Log("Action type's name is " + e.actionMemberInfo.Name + " " + e.actionMemberInfo.DeclaringType);
            if (actionType == null) continue;
            Debug.Log("Action type is not null");

            if (IsValidActionType(actionType, out var args))
            {
                Debug.Log("Action type is valid");
                foreach (GameObject go in hierarchy)
                    if (go.name == goName)
                        foreach (var comp in go.GetComponents<Component>())
                            if (comp.GetType().Name == compName)
                            {
                                Debug.Log("Component found");
                                var feature = (Feature)featureInfo.GetValue(comp);
                                object actionToBe = info.GetValue(feature);

                                Debug.Log("Switch-case reached");

                                switch (args)
                                {
                                    case 0:
                                        Debug.Log("Case 0");
                                        var action = (Action)actionToBe;
                                        action += () => e.Event?.Invoke();
                                        break;
                                    case 1:
                                        Debug.Log("Case 1");
                                        var action1 = (Action)actionToBe;
                                        action1 += () => e.Event?.Invoke();
                                        break;
                                    case 2:
                                        Debug.Log("Case 2");
                                        var action2 = (Action)actionToBe;
                                        action2 += () => e.Event?.Invoke();
                                        break;
                                }

                            }
                                    
            }
        }
    }











    public void AddEvent(MemberInfo actionInfo, MemberInfo featureInfo, string name)
    {
        var wrapper = new UnityEventWrapper(name, new UnityEvent(), actionInfo, featureInfo);
        Events.Add(wrapper);
    }

    // First MemberInfo is the action's memberInfo inside the feature. The second MemberInfo is the feature's info inside the Component
    public List<Tuple<string, MemberInfo, MemberInfo>> GetAvailableActions()
    {
        var hierarchy = GetChildren(gameObject);
        var components = GetComponentsFromGameObjects(hierarchy);
        List<Tuple<string, MemberInfo, MemberInfo>> result = new();

        foreach (var component in components)
        {
            var features = GetFeatures(component);
            foreach (var feature in features)
            {
                result.AddRange(GetActions(feature));
            }
        }
        return result;
    }
    private bool IsValidActionType(Type type) => IsValidActionType(type, out var temp);
    private bool IsValidActionType(Type type, out int args)
    {
        args = 0;
        if (type == typeof(System.Action)) return true;
        Type generic = null;
        if (type.IsGenericTypeDefinition) generic = type;
        else if (type.IsGenericType) generic = type.GetGenericTypeDefinition();
        if (generic == null) return false;
        args = 1;
        if (generic == typeof(System.Action<>)) return true;
        args = 2;
        if (generic == typeof(System.Action<,>)) return true;
        return false;
    }
    private List<Tuple<string, MemberInfo, MemberInfo>> GetActions(Tuple<string, MemberInfo> feature)
    {
        List<Tuple<string, MemberInfo, MemberInfo>> result = new();
        string name = feature.Item1 + "/" + feature.Item2.DeclaringType.Name + "." + feature.Item2.Name;

        // Get the declaring feature type

        Type featureType = new PropertyOrFieldInfo(feature.Item2).Type;

        foreach (var member in featureType.GetMembers())
        {
            Type type = new PropertyOrFieldInfo(member).Type;
            if (type == null)
                continue;
            if (IsValidActionType(type))
            {
                var tuple = new Tuple<string, MemberInfo, MemberInfo>(name, member, feature.Item2);
                result.Add(tuple);
            }
        }
        return result;
    }

    

    private List<Tuple<string, MemberInfo>> GetFeatures(Component component)
    {
        List<Tuple<string, MemberInfo>> result = new();
        if (component is IFeatureCollection)
        {
            foreach (MemberInfo member in component.GetType().GetMembers())
            {
                Type type = new PropertyOrFieldInfo(member).Type;
                if (type == null)
                    continue;
                if (typeof(Feature).IsAssignableFrom(type))
                {
                    var tuple = new Tuple<string, MemberInfo>(component.name, member);
                    result.Add(tuple);
                }
            }
        }
        return result;
    }
    private List<GameObject> GetChildren(GameObject go, bool addBaseGameObject = true)
    {
        List<GameObject> children = new List<GameObject>();

        if (addBaseGameObject)
            children.Add(go);

        for (int i = 0; i < go.transform.childCount; i++)
        {
            children.AddRange(GetChildren(go.transform.GetChild(i).gameObject, true));
        }
        return children;
    }
    private List<Component> GetComponentsFromGameObjects(List<GameObject> gameObjects)
    {
        var components = new List<Component>();
        foreach (GameObject go in gameObjects)
        {
            components.AddRange(go.GetComponents<Component>());
        }
        return components;
    }
}

[System.Serializable]
public struct UnityEventWrapper
{
    public string name;
    public UnityEvent Event;
    public MemberInfo actionMemberInfo;
    public MemberInfo featureMemberInfo;

    public UnityEventWrapper(string name, UnityEvent _event, MemberInfo actionInfo, MemberInfo featureInfo)
    {
        this.name = name;
        this.Event = _event;
        this.actionMemberInfo = actionInfo;
        this.featureMemberInfo = featureInfo;
    }
}

public class PropertyOrFieldInfo
{
    MemberInfo reference;
    FieldInfo field => reference as FieldInfo;
    PropertyInfo property => reference as PropertyInfo;

    public Type Type { get 
        {
                if (field != null) 
                    return field.FieldType;
                if (property != null) 
                    return property.PropertyType;
                return null;
            } }
    public PropertyOrFieldInfo(MemberInfo info)
    {
        reference = info;
    }
    public object GetValue(object obj)
    {
        if (field != null) return field.GetValue(obj);
        if (property != null) return property.GetValue(obj);
        return null;
    }
    public void SetValue(object obj, object value)
    {
        if (field != null) field.SetValue(obj, value);
        else if (property != null) property.SetValue(obj, value);
    }

}