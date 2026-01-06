using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cooking;
using Clients;
using Controller;
using UI;

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
        
        public Dictionary<Type, IManager> Managers => _managers;
        
        
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
            typeof(Oven),
            typeof(TrashCan),
            typeof(Meal),
            typeof(Blender),
            typeof(SimpleClientSpawner),
            typeof(PlayerStart),
            typeof(PlayerInterface),
            typeof(OvenSetup),
            typeof(PlayerInterface),
            typeof(Entrance),
            typeof(PlayerController),
            typeof(UITarget),
            typeof(ItemSpawner),
            typeof(Blender)

        };
        private Dictionary<Type, List<MonoBehaviour>> _elementDict;
        private Dictionary<Type, IManager> _managers;

        private static ReferenceManager _instance;
        private static SO_AssetReferences _assetReferences;
        #endregion

        #region Methods

        public ReferenceManager() 
        {
            _elementDict = new();
            _managers = new();
            ScanHierarchy();
            //DebugTrackedObjectsCount();
        }

        public static void Reset()
        {
            _instance = new();
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

        public void RemoveManager<T>() where T : IManager
        {
            if (!(_managers.ContainsKey(typeof(T)) && _managers[typeof(T)] != null))
            {
                Debug.LogWarning("A manager '" + typeof(T).FullName + "' tried to unregister on the ReferenceManager, but no instance was already registered.");
            }
            _managers[typeof(T)] = null;
        }
        public void RemoveManager<T>(T manager) where T: IManager
        {
            RemoveManager<T>();
        }
        public void RemoveAllManagers()
        {
            _managers.Clear();
        }

        public T GetManager<T>() where T : class, IManager
        {
            if (!_managers.ContainsKey(typeof(T)) || _managers[typeof(T)] == null)
            {
                Debug.LogWarning("No manager of type " + typeof(T).FullName + " was found. Key contained : " + (_managers.ContainsKey(typeof(T))?"true":"false"));
                return default(T);
            }
            return _managers[typeof(T)] as T;
        }
        public static T Get<T>() where T : class, IManager => Instance.GetManager<T>();

        public bool TryGetManager<T>(out T manager) where T : class, IManager
        {
            manager = GetManager<T>();
            return manager != null;
        }

        public List<T> GetElementsOfType<T>() where T : class
        {
            if (!_elementDict.ContainsKey(typeof(T))) return new List<T>();
            return _elementDict[typeof(T)].Select((mb) => mb as T).ToList();
        }

        public T GetElementOfType<T>() where T : class => GetElementsOfType<T>().FirstOrDefault();

        public void TryAddElement<T>(T element) where T : MonoBehaviour
        {
            if (!_elementDict.ContainsKey(typeof(T)))
                _elementDict.Add(typeof(T), new List<MonoBehaviour>());
            if (!_elementDict[typeof(T)].Contains(element))
                _elementDict[typeof(T)].Add(element);
        }

        public void ScanHierarchy(bool clearDictBeforeScan=true)
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
        
        public void ResetInstance()
        {
            _instance = null;
        }
        
        #endregion
    }
}