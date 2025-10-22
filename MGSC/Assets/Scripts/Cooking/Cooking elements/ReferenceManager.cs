using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cooking;
using Client;

/// <summary>
/// The reference manager holds a reference to every kitchen element, and eases reference setup
/// </summary>
public class ReferenceManager
{
    #region Properties
    // Lazy initialization
    public static ReferenceManager Instance { get { if (_instance == null) _instance = new ReferenceManager(); return _instance;}}
        
    #endregion

    #region Fields

    private Type[] _trackedTypes =
    {
        typeof(OrderingCounter),
        typeof(SittingCounter),
        typeof(Counter),
        typeof(Item),
        typeof(Ingredient),
        typeof(Furnace),
        typeof(TrashCan),
        typeof(Meal),
        typeof(SimpleClientSpawner),

    };
    private Dictionary<Type, List<MonoBehaviour>> _elementDict = new();

    private static ReferenceManager _instance;
    #endregion

    #region Methods

    public ReferenceManager()
    {
        ScanHierarchy();
        DebugTrackedObjectsCount();
    }

    public List<T> GetElementsOfType<T>() where T : class
    {
        if (!_elementDict.ContainsKey(typeof(T))) return null;
        return _elementDict[typeof(T)].Select((mb) => mb as T).ToList();
    }

    public void ScanHierarchy(bool clearDictBeforeScan=false)
    {
        if (clearDictBeforeScan)
            _elementDict.Clear();

        foreach (var type in _trackedTypes)
        {
            if (!_elementDict.ContainsKey(type))
                _elementDict.Add(type, new List<MonoBehaviour>());

            _elementDict[type].InsertRange(0, (IEnumerable<MonoBehaviour>)GameObject.FindObjectsByType(type, FindObjectsSortMode.None) );
        }
    }

    public void DebugTrackedObjectsCount()
    {
        string toPrint = "References tracked :\n";

        foreach (var kvp  in _elementDict)
        {
            toPrint += kvp.Key.Name + " : ";
            toPrint += kvp.Value?.Count + "\n";
        }

        Debug.Log(toPrint);
    }
    #endregion
}