using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

/// <summary>
/// The reference manager holds a reference to every kitchen element, and eases reference setup
/// </summary>
public class Reference
{
    #region Properties
    // Lazy initialization
    protected static Reference Instance { get { if (_instance == null) _instance = new Reference(); return _instance; } }
    public static SO_AssetReferences AssetReferences
    {
        get
        {
            if (_assetReferences == null)
                Debug.LogError("[ReferenceManager] : No asset references SO found. Make sure it was correctly referenced.");
            return _assetReferences;
        }
        set => _assetReferences = value;
    }

    public static Dictionary<Type, IManager> Managers => Instance._managers;


    #endregion

    #region Fields

    private Type[] _trackedTypes = TrackedTypes.GetTypes();
    private Dictionary<Type, List<object>> _elementDict;
    private Dictionary<Type, IManager> _managers;

    protected static Reference _instance;
    private static SO_AssetReferences _assetReferences;
    #endregion

    #region Methods

    public Reference()
    {
        _elementDict = new();
        _managers = new();
        ScanHierarchy_Instance();
    }

    /// <summary>
    /// Recreates a new instance, clearing all the references held so far.
    /// </summary>
    public static void Reset()
    {
        _instance = new();
    }
    /// <summary>
    /// Store the reference to this manager to be accessible everywhere
    /// </summary>
    /// <typeparam name="T">Manager type, implements IManager</typeparam>
    /// <param name="manager"></param>
    public static void SetManager<T>(T manager) where T : IManager => Instance.SetManager_Instance(manager);
    /// <summary>
    /// Remove the manager associated to this type from the global reference index.
    /// This will NOT break or update existing references that other scripts might hold to this manager.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void RemoveManager<T>() where T : IManager => Instance.RemoveManager_Instance<T>();
    /// <summary>
    /// Remove the manager associated to this type from the global reference index.
    /// This will NOT break or update existing references that other scripts might hold to this manager.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void RemoveManager<T>(T manager) where T : IManager => Instance.RemoveManager_Instance<T>();
    /// <summary>
    /// Removes all the managers from the global reference index.
    /// This will NOT break or update existing references that other scripts might hold to these managers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void RemoveAllManagers() => Instance.RemoveAllManagers_Instance();
    /// <summary>
    /// Get a reference to a manager that registered to the reference index
    /// </summary>
    /// <typeparam name="T">Type of the requested manager</typeparam>
    /// <returns></returns>
    public static T GetManager<T>() where T : class, IManager => Instance.GetManager_Instance<T>();
    /// <summary>
    /// Get a reference to a manager that registered to the reference index
    /// </summary>
    /// <typeparam name="T">Type of the requested manager</typeparam>
    /// <returns></returns>
    public static T GetManager<T>(out T manager) where T : class, IManager => manager = GetManager<T>();
    /// <summary>
    /// Tries to retrieve a registered manager
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="manager"></param>
    /// <returns>Wether or not the manager could be found. If false, manager will be null</returns>
    public static bool TryGetManager<T>(out T manager) where T : class, IManager => Instance.TryGetManager_Instance(out manager);
    /// <summary>
    /// Retrieves all the objects of this specific class that are in the reference index.
    /// To ensure all objects from the current scene are indexed, call Reference.ScanHierarchy().
    /// The retrieved object cannot be of an inheriting class.
    /// The given type must be marked as a tracked type to be retrieved.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [Obsolete] public static List<T> GetAll<T>() where T : class => Instance.GetElementsOfType_Instance<T>();
    [Obsolete] public static List<T> GetAll<T>(List<T> output) where T : class => output = Instance.GetElementsOfType_Instance<T>();
    /// <summary>
    /// Tries to retrieve an object of a given specific type from the scene.
    /// The type must be marked as a tracked type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    [Obsolete] public static bool TryGet<T>(out T obj) where T : class
    {
        obj = Instance.GetElementOfType_Instance<T>();
        return obj != null;
    }
    /// <summary>
    /// Get the first indexed object of the given type.
    /// The type must be marked as a tracked type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    [Obsolete] public static T Get<T>(out T obj) where T : class
    {
        obj = Get<T>();
        return obj;
    }
    /// <summary>
    /// Get the first indexed object of the given type.
    /// The type must be marked as a tracked type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns>Said object</returns>
    [Obsolete] public static T Get<T>() where T : class => Instance.GetElementOfType_Instance<T>();
    /// <summary>
    /// Explicitly register an object for it to be accessible anywhere.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    [Obsolete] public static void TryAdd<T>(T obj) where T : class => Instance.TryAddElement_Instance(obj);
    /// <summary>
    /// Explicitly unregisters an object for it not to be accessible anymore.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    [Obsolete] public static void TryRemove<T>(T obj) where T : class => Instance.TryRemoveElement_Instance(obj);
    /// <summary>
    /// Scans the whole scene and stores a reference to every object whose type is marked as a tracked type (see TrackedTypes).
    /// Can be slow depending on the amount of GameObjects to scan
    /// </summary>
    /// <param name="clearDictBeforeScan"></param>
    [Obsolete] public static void ScanHierarchy(bool clearDictBeforeScan = true)
    {
        Instance.ScanHierarchy_Instance(clearDictBeforeScan);
    }
    /// <summary>
    /// Sets the current Reference instance to a brand new one with no references.
    /// </summary>
    public static void ResetInstance()
    {
        _instance = null;
    }
    /// <summary>
    /// Logs to the Unity console how many objects of each type is registered to Reference, excluding managers
    /// </summary>
    [Obsolete]
    public static void DebugTrackedObjectsCount()
    {
        string toPrint = "References tracked :\n";

        foreach (var kvp in Instance._elementDict)
        {
            toPrint += kvp.Key.Name + " : ";
            toPrint += kvp.Value?.Count + "\n";
        }

        Debug.Log(toPrint);
    }




    protected void ScanHierarchy_Instance(bool clearDictBeforeScan = true)
    {
        if (clearDictBeforeScan)
            _elementDict.Clear();

        foreach (var type in _trackedTypes)
        {
            if (!_elementDict.ContainsKey(type))
                _elementDict.Add(type, new List<object>());

            _elementDict[type].InsertRange(0, GameObject.FindObjectsByType(type, FindObjectsSortMode.None));
        }
    }
    protected void SetManager_Instance<T>(T manager) where T : IManager
    {
        if (manager == null) throw new ArgumentNullException(nameof(manager));
        if (_managers.ContainsKey(typeof(T)) && _managers[typeof(T)] != null)
        {
            Debug.LogWarning("A manager '" + nameof(manager) + "' tried to register on the ReferenceManager, but an instance was already registered.");
        }
        _managers[typeof(T)] = manager;
    }
    protected void RemoveManager_Instance<T>() where T : IManager
    {
        if (!(_managers.ContainsKey(typeof(T)) && _managers[typeof(T)] != null))
        {
            Debug.LogWarning("A manager '" + typeof(T).FullName + "' tried to unregister on the ReferenceManager, but no instance was already registered.");
        }
        _managers[typeof(T)] = null;
    }
    protected void RemoveAllManagers_Instance() => _managers.Clear();
    protected T GetManager_Instance<T>() where T : class, IManager
    {
        if (!_managers.ContainsKey(typeof(T)) || _managers[typeof(T)] == null)
        {
            Debug.LogWarning("No manager of type " + typeof(T).FullName + " was found. Key contained : " + (_managers.ContainsKey(typeof(T)) ? "true" : "false"));
            return default(T);
        }
        return _managers[typeof(T)] as T;
    }
    protected bool TryGetManager_Instance<T>(out T manager) where T : class, IManager
    {
        manager = GetManager_Instance<T>();
        return manager != null;
    }

    protected List<T> GetElementsOfType_Instance<T>() where T : class
    {
        if (!_elementDict.ContainsKey(typeof(T))) return new List<T>();
        return _elementDict[typeof(T)].Select((mb) => mb as T).ToList();
    }
    protected T GetElementOfType_Instance<T>() where T : class => GetElementsOfType_Instance<T>().FirstOrDefault();

    protected void TryAddElement_Instance<T>(T element) where T : class
    {
        if (!_elementDict.ContainsKey(typeof(T)))
            _elementDict.Add(typeof(T), new List<object>());
        if (!_elementDict[typeof(T)].Contains(element))
            _elementDict[typeof(T)].Add(element);
    }
    protected void TryRemoveElement_Instance<T>(T element) where T : class
    {
        if (!_elementDict.ContainsKey(typeof(T)))
            return;
        if (!_elementDict[typeof(T)].Contains(element))
            return;
        _elementDict[typeof(T)].Remove(element);
    }




    #endregion
}
