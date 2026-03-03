using Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Events;

public class EventDisplay : MonoBehaviour, IFeatureCollection
{
    [SerializeField] public List<UnityEventWrapper> Events = new();



    ProfilerMarker bindEventProfilerMarker = new ProfilerMarker("EventDisplay.BindEventsWithReflection");

    public FeatureCollection Collection { get; set; } = new();
    public BaseEvents BaseEvents { get; set; }

    private void Awake()
    {
        BaseEvents = new(this);
    }
    void Start()
    {
        BindEvents();
        Collection.LateSetup();
    }
    void Update()
    {
        Collection.Update();
    }

    public bool IsValid(UnityEventWrapper wrapper)
    {
        if (wrapper.ActionPath == null) return false;
        if (!wrapper.ActionPath.IsValid()) return false;

        return true;
    }

    private void SkippedEventWarning(UnityEventWrapper e) => 
        Debug.LogWarning("The UnityEvent " + e.ActionPath.FullName + " couldn't be bound");
    private void BindEvents()
    {
        bindEventProfilerMarker.Begin();

        foreach (var e in Events)
        {
            if (IsValidActionType(e.ActionPath.Type))
            {
                Component comp = e.ActionPath.FeaturePath.OwningComponent;
                if (comp != null)
                {
                    Feature feature;
                    if (!e.ActionPath.FeaturePath.TryGetInstance(out feature))
                    {
                        SkippedEventWarning(e);
                        continue;
                    }

                    PropertyOrFieldInfo actionInfo = new PropertyOrFieldInfo(e.ActionPath.GetMemberInfo());
                    if (!TryBindCallbackToFeature(feature, () => e.Event?.Invoke(), actionInfo))
                    {
                        SkippedEventWarning(e);
                        continue;
                    }
                }
            }
            else
                SkippedEventWarning(e);
        }
        bindEventProfilerMarker.End();
    }

    /// <summary>
    /// Binds value to the Action described by actionInfo that is a member of feature.
    /// </summary>
    /// <param name="feature"></param>
    /// <param name="value"></param>
    /// <param name="actionInfo"></param>
    /// <returns></returns>
    private bool TryBindCallbackToFeature(Feature feature, Action value, PropertyOrFieldInfo actionInfo)
    {
        object actionToBe = actionInfo.GetValue(feature);
        if (actionToBe == null)
        {
            actionToBe = NewGenericAction(actionInfo.Type);
            if (actionToBe == null)
            {
                return false;
            }
        }
        BindCallbackToAction(ref actionToBe, value);
        actionInfo.SetValue(feature, actionToBe);
        return true;
    }

    public void EmptyMethod0() { }
    public void EmptyMethod1<T>() { }
    public void EmptyMethod2<T1, T2>() { }
    public void EmptyMethod3<T1, T2, T3>() { }
    private object NewGenericAction(Type actionType)
    {
        object result = null;

        int argCount = actionType.GetGenericArguments().Length;
        MethodInfo emptyMethod = this.GetType().GetMethod("EmptyMethod" + argCount) ;

        if (argCount > 0)
            emptyMethod = emptyMethod.MakeGenericMethod(actionType.GetGenericArguments());

        result = Delegate.CreateDelegate(actionType, this, emptyMethod);

        return result;
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
    /// <summary>
    /// Binds a non-generic callback to a generic or non-generic action
    /// </summary>
    /// <param name="action"></param>
    /// <param name="callback"></param>
    void BindCallbackToAction(ref object action, Action callback)
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
            action = closedMethod.Invoke(this, new object[] { action, callback });
        }
    }








    public void AddEvent(UnityEventWrapper wrapper)
    {
        Events.Add(wrapper);
    }

    // First MemberInfo is the action's memberInfo inside the feature. The second MemberInfo is the feature's info inside the Component
    public List<ActionPath> GetAvailableActions()
    {
        var hierarchy = GetChildren(gameObject);
        var components = GetComponentsFromGameObjects(hierarchy);
        List<ActionPath> result = new();

        foreach (var component in components)
        {
            if (component is IFeatureCollection)
            {
                var features = GetFeatures(component.GetType());
                foreach (var feature in features)
                {
                    feature.OwningComponent = component;
                    result.AddRange(feature.GetActions());
                }
            }
        }
        return result;
    }
    private bool IsValidActionType(Type type)
    {
        if (type == null) return false;
        if (type == typeof(System.Action)) return true;
        Type generic = null;
        if (type.IsGenericTypeDefinition) generic = type;
        else if (type.IsGenericType) generic = type.GetGenericTypeDefinition();
        if (generic == null) return false;
        if (generic == typeof(System.Action<>)) return true;
        if (generic == typeof(System.Action<,>)) return true;
        if (generic == typeof(System.Action<,,>)) return true;
        return false;
    }
    

    

    private List<FeaturePath> GetFeatures(Type targetType, List<FeaturePath> currentPath=null)
    {
        List<FeaturePath> result = new();

        // Only retrieve features if the given type is a feature collection
        if (!typeof(IFeatureCollection).IsAssignableFrom(targetType))
            return result;

        if (currentPath == null) currentPath = new();

        // Scan the type
        foreach (MemberInfo member in targetType.GetMembers())
        {
            Type type = new PropertyOrFieldInfo(member).Type;
            if (type == null)
                continue;
            if (typeof(Feature).IsAssignableFrom(type))
            {
                // Add the detected feature to the list
                FeaturePath path = new(member.Name, new SerializableSystemType(type), currentPath, null);
                result.Add(path);

                // Add the child features as well
                List<FeaturePath> newPath = currentPath.Concat(new FeaturePath[] { path }).ToList();
                result.AddRange(GetFeatures(type, newPath));
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
    public string name => ActionPath.FullName.Replace("/", " -> ");
    public ActionPath ActionPath;
    public UnityEvent Event;


    public UnityEventWrapper(ActionPath actionPath, UnityEvent _event)
    {
        this.ActionPath = actionPath;
        this.Event = _event;
    }

    public MemberInfo GetMember(Type type, string name)
    {
        var members = type.GetMember(name);
        if (members == null || members.Length == 0)
            return null;
        return members[0];
    }
}
