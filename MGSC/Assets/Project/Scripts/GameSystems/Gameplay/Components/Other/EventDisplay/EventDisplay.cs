using Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Profiling;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class EventDisplay : MonoBehaviour, IFeatureCollection
{
    [SerializeField] public List<UnityEventWrapper> Events = new();
    private static List<ActionPath> _cachedGlobalActions = null;
    
    ProfilerMarker bindEventProfilerMarker = new ProfilerMarker("EventDisplay.BindEventsWithReflection");

    public FeatureCollection Collection { get; set; } = new();
    public BaseEvents BaseEvents { get; set; }


    public void InvokeCustomEvent0()=>BaseEvents.InvokeCustomEvent0();
    public void InvokeCustomEvent1()=>BaseEvents.InvokeCustomEvent1();
    public void InvokeCustomEvent2()=>BaseEvents.InvokeCustomEvent2();
    public void InvokeCustomEvent3()=>BaseEvents.InvokeCustomEvent3();
    
    private void Awake()
    {
        BaseEvents = new(this);
    }
    void Start()
    {
        BindEvents();
        Collection.LateSetup();
    }

    public bool IsValid(UnityEventWrapper wrapper)
    {
        if (wrapper.ActionPath == null) return false;
        if(wrapper.ActionPath.source == ActionPath.ActionSource.Global) return true;
        if (!wrapper.ActionPath.IsValid()) return false;

        return true;
    }

    private void SkippedEventWarning(UnityEventWrapper e) => Debug.LogWarning("The UnityEvent " + e.ActionPath.FullName + " couldn't be bound");
    private void BindEvents()
    {
        bindEventProfilerMarker.Begin();

        foreach (var e in Events)
        {
            if(!IsValid(e)) {SkippedEventWarning(e); continue;}

            if (e.ActionPath.source == ActionPath.ActionSource.Local)
            {
                UnityEngine.Object comp = e.ActionPath.FeaturePath.OwningObject;
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
                else
                    SkippedEventWarning(e);
            }
            else
            {
                BindGlobalEvent(e);
            }
            
            
        }
        bindEventProfilerMarker.End();
    }

    private static Dictionary<string, PropertyOrFieldInfo> _globalEventMembers;

    private static void BuildGlobalEventLookup()
    {
        _globalEventMembers = new();
        _cachedGlobalActions = new();

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types; }

            foreach (var type in types)
            {
                if (type == null) continue;

                foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Static))
                {
                    var attr = member.GetCustomAttribute<GlobalEventAttribute>();
                    if (attr == null) continue;

                    var info = new PropertyOrFieldInfo(member);
                    if (info.Type == null || !IsValidActionType(info.Type)) continue;

                    _globalEventMembers[attr.Name] = info;
                    _cachedGlobalActions.Add(new ActionPath(attr.Name, info.Type));
                }
            }
        }
    }

    private void BindGlobalEvent(UnityEventWrapper e)
    {
        if (_globalEventMembers == null) BuildGlobalEventLookup();

        string targetName = e.ActionPath.ActionVariableName;
        if (_globalEventMembers.TryGetValue(targetName, out var info))
            TryBindCallbackToFeature(null, () => e.Event?.Invoke(), info);
        else
            SkippedEventWarning(e);
    }

    

    /// <summary>
    /// Binds value to the Action described by actionInfo that is a member of feature.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="value"></param>
    /// <param name="actionInfo"></param>
    /// <returns></returns>
    private bool TryBindCallbackToFeature(object target, Action value, PropertyOrFieldInfo actionInfo)
    {
        object actionToBe = actionInfo.GetValue(target);
        if (actionToBe == null)
        {
            actionToBe = NewGenericAction(actionInfo.Type);
            if (actionToBe == null)
            {
                return false;
            }
        }
        BindCallbackToAction(ref actionToBe, value);
        actionInfo.SetValue(target, actionToBe);
        return true;
    }

    public void EmptyMethod0() { }
    public void EmptyMethod1<T>(T arg) { }
    public void EmptyMethod2<T1, T2>(T1 arg1, T2 arg2) { }
    public void EmptyMethod3<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3) { }
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
        // Local actions
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
                    feature.OwningObject = component;
                    result.AddRange(feature.GetActions());
                }
            }
        }

        
        // Global Actions
        if(_cachedGlobalActions != null) result.AddRange(_cachedGlobalActions);
        else
        {
            BuildGlobalActionCache();
            result.AddRange(_cachedGlobalActions);
        }
        
        
        return result;
    }
    
    
    public static void BuildGlobalActionCache()
    {
        _cachedGlobalActions = new();
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in asm.GetTypes())
            {
                foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Static))
                {
                    var attr = member.GetCustomAttribute<GlobalEventAttribute>();
                    if (attr == null) continue;

                    if (!member.IsStatic())
                    {
                        Debug.LogWarning($"[GlobalEvent] field '{member.Name}' on '{type.Name}' must be static. Skipping.");
                    }
                    
                    var info = new PropertyOrFieldInfo(member);
                    if(info.Type == null) continue;
                    if (!IsValidActionType(info.Type)) continue;
                    _cachedGlobalActions.Add(new ActionPath(attr.Name, info.Type));
                }
            }
            
        }
        
    }
    private static bool IsValidActionType(Type type)
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
