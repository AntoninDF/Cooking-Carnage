using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cooking;
using Clients;
using UnityEngine.Rendering;
using Controller;

namespace GameSystem
{

    /// <summary>
    /// The reference manager holds a reference to every kitchen element, and eases reference setup
    /// </summary>
    public class ReferenceManager
    {
        #region Properties
        // Lazy initialization
        public static ReferenceManager Instance { get { if (_instance == null) _instance = new ReferenceManager(); return _instance;}}
        public static SO_AssetReferences AssetReferences { get; set; }
        
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
            typeof(PlayerStart),
            typeof(PlayerInterface),

        };
        private Dictionary<Type, List<MonoBehaviour>> _elementDict;
        private Dictionary<Type, IManager> _managers;

        private static ReferenceManager _instance;
        #endregion

        #region Methods

        public ReferenceManager() 
        {
            _elementDict = new();
            _managers = new();
            ScanHierarchy();
            //DebugTrackedObjectsCount();
        }

        public void SetManager<T>(T manager) where T : IManager
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (_managers.ContainsKey(typeof(T)) && _managers[typeof(T)] != null)
            {
                Debug.LogWarning("A manager '" + nameof(manager) + "' tried to register on the ReferenceManager, but an instance was already registered.");
            }
            _managers[typeof(T)] = manager;
        }

        public T GetManager<T>() where T : class, IManager
        {
            if (!_managers.ContainsKey(typeof(T)) || _managers[typeof(T)] == null)
            {
                Debug.LogWarning("No manager of type " + nameof(T) + " was found. Key contained : " + (_managers.ContainsKey(typeof(T))?"true":"false"));
                return default(T);
            }
            return _managers[typeof(T)] as T;
        }
        public bool TryGetManager<T>(out T manager) where T : class, IManager
        {
            manager = GetManager<T>();
            return manager != null;
        }

        public List<T> GetElementsOfType<T>() where T : class
        {
            if (!_elementDict.ContainsKey(typeof(T))) return null;
            return _elementDict[typeof(T)].Select((mb) => mb as T).ToList();
        }

        public T GetElementOfType<T>() where T : class => GetElementsOfType<T>().FirstOrDefault();

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
}