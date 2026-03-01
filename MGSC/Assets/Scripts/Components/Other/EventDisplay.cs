using Features;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Events;

public class EventDisplay : MonoBehaviour
{
    [SerializeField] public List<UnityEventWrapper> Events = new();



    ProfilerMarker bindEventProfilerMarker = new ProfilerMarker("EventDisplay.BindEventsWithReflection");
    void Start()
    {
        BindEvents();
    }

    public bool IsValid(UnityEventWrapper eventWrapper)
    {

        MemberInfo actionMember = GetMember(eventWrapper.featureType.SystemType, eventWrapper.actionMemberInfoName);
        MemberInfo featureMember = GetMember(eventWrapper.componentType.SystemType, eventWrapper.featureMemberInfoName);
        if (actionMember == null || featureMember == null)
            return false;
        return true;
    }

    private void BindEvents()
    {
        bindEventProfilerMarker.Begin();
        var hierarchy = GetChildren(gameObject);
        var components = GetComponentsFromGameObjects(hierarchy);
        foreach (var e in Events)
        {
            // Parse the name to get the GameObject's name and the component's name
            string[] parsedName = e.name.Split('/', '-');
            if (parsedName.Length < 4) continue;
            string goName = parsedName[0];
            string compName = parsedName[1];
            string featureName = parsedName[2];
            string actionName = parsedName[3];

            // Get the member infos on the component and on it's feature
            MemberInfo actionMember = GetMember(e.featureType.SystemType, e.actionMemberInfoName);
            MemberInfo featureMember = GetMember(e.componentType.SystemType, e.featureMemberInfoName);
            var actionInfo = new PropertyOrFieldInfo(actionMember);
            var featureInfo = new PropertyOrFieldInfo(featureMember);


            Type actionType = actionInfo.Type;
            if (actionType == null) continue;

            if (IsValidActionType(actionType, out var args))
            {
                foreach (GameObject go in hierarchy)
                    if (go.name == goName)
                        foreach (var comp in go.GetComponents<Component>())
                            if (comp.GetType().Name == compName)
                            {
                                var feature = (Feature)featureInfo.GetValue(comp);
                                object actionToBe = actionInfo.GetValue(feature);
                                if (actionToBe == null) continue;

                                BindActions(ref actionToBe, () => e.Event?.Invoke());
                                actionInfo.SetValue(feature, actionToBe);
                            }
                                    
            }
        }
        bindEventProfilerMarker.End();
    }

    public object BindActionsGeneric0(object action, Action boundDelegate)
    {
        Action finalAction = (Action)action;
        finalAction += () => boundDelegate?.Invoke();
        return finalAction;
    }

    public object BindActionsGeneric1<T1>(object action, Action boundDelegate)
    {
        Action<T1> finalAction = (Action<T1>)action;
        finalAction += (a) => boundDelegate?.Invoke();
        return finalAction;
    }

    public object BindActionsGeneric2<T1, T2>(object action, Action boundDelegate)
    {
        Action<T1, T2> finalAction = (Action<T1, T2>)action;
        finalAction += (a,b) => boundDelegate?.Invoke();
        return finalAction;
    }

    public object BindActionsGeneric3<T1, T2, T3>(object action, Action boundDelegate)
    {
        Action<T1, T2, T3> finalAction = (Action<T1, T2, T3>)action;
        finalAction += (a,b,c) => boundDelegate?.Invoke();
        return finalAction;
    }

    void BindActions(ref object action, Action boundDelegate)
    {
        Type actionType = action.GetType();
        Type[] typeArgs = actionType.GetGenericArguments();
        MethodInfo method = null;
        switch (typeArgs.Length)
        {
            case 0:
                method = this.GetType().GetMethod("BindActionsGeneric0");
                break;
            case 1:
                method = this.GetType().GetMethod("BindActionsGeneric1");
                break;
            case 2:
                method = this.GetType().GetMethod("BindActionsGeneric2");
                break;
            case 3:
                method = this.GetType().GetMethod("BindActionsGeneric3");
                break;
        }

        if (method != null)
        {
            MethodInfo closedMethod = method;
            if (typeArgs.Length > 0)
                closedMethod = method.MakeGenericMethod(typeArgs);
            action = closedMethod.Invoke(this, new object[] { action, boundDelegate });
        }
    }








    public void AddEvent(UnityEventWrapper wrapper)
    {
        Events.Add(wrapper);
    }

    // First MemberInfo is the action's memberInfo inside the feature. The second MemberInfo is the feature's info inside the Component
    public List<(string actionName, MemberInfo actionInfo, MemberInfo featureInfo)> GetAvailableActions()
    {
        var hierarchy = GetChildren(gameObject);
        var components = GetComponentsFromGameObjects(hierarchy);
        List<(string actionName, MemberInfo actionInfo, MemberInfo featureInfo)> result = new();

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
    private List<(string actionName, MemberInfo actionInfo, MemberInfo featureInfo)> GetActions((string owningObject, MemberInfo info) feature)
    {
        List<(string actionName, MemberInfo actionInfo, MemberInfo featureInfo)> result = new();
        string name = feature.owningObject + "/" + feature.info.DeclaringType.Name + "-" + feature.info.Name;

        // Get the declaring feature type

        Type featureType = new PropertyOrFieldInfo(feature.Item2).Type;

        foreach (var member in featureType.GetMembers())
        {
            Type type = new PropertyOrFieldInfo(member).Type;
            if (type == null)
                continue;
            if (IsValidActionType(type))
            {
                var tuple = (name, member, feature.Item2);
                result.Add(tuple);
            }
        }
        return result;
    }

    

    private List<(string owningObject, MemberInfo featureInfo)> GetFeatures(Component component)
    {
        List<(string owningObject, MemberInfo featureInfo)> result = new();
        if (component is IFeatureCollection)
        {
            foreach (MemberInfo member in component.GetType().GetMembers())
            {
                Type type = new PropertyOrFieldInfo(member).Type;
                if (type == null)
                    continue;
                if (typeof(Feature).IsAssignableFrom(type))
                {
                    var tuple = (component.name, member);
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

    public MemberInfo GetMember(Type type, string name)
    {
        var members = type.GetMember(name);
        if (members == null || members.Length == 0)
            return null;
        return members[0];
    }
}

[System.Serializable]
public struct UnityEventWrapper
{
    public string name;
    public UnityEvent Event;
    public string actionMemberInfoName;
    public string featureMemberInfoName;
    public SerializableSystemType featureType;
    public SerializableSystemType componentType;


    public UnityEventWrapper(string eventName, UnityEvent _event, string actionInfoName, string featureInfoName, Type featureType, Type componentType)
    {
        this.name = eventName;
        this.Event = _event;
        this.actionMemberInfoName = actionInfoName;
        this.featureMemberInfoName = featureInfoName;
        this.featureType = new SerializableSystemType(featureType);
        this.componentType = new SerializableSystemType(componentType);
    }

    public MemberInfo GetMember(Type type, string name)
    {
        var members = type.GetMember(name);
        if (members == null || members.Length == 0)
            return null;
        return members[0];
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
        if (field != null)
        {
            Debug.Log("Getting value through field on object " + obj);
            return field.GetValue(obj);
        }
        if (property != null)
        {
            Debug.Log("Getting value through property on object " + obj);
            return property.GetValue(obj);
        }
        Debug.Log("GetValue is null because the reference member is invalid");
        return null;
    }
    public void SetValue(object obj, object value)
    {
        if (field != null) field.SetValue(obj, value);
        else if (property != null) property.SetValue(obj, value);
    }

}