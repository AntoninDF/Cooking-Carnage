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
    public static Dictionary<Type, IManager> Managers => _managers;
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



    #endregion

    #region Fields

    private static Dictionary<Type, IManager> _managers;
    private static SO_AssetReferences _assetReferences;
    #endregion

    #region Methods

    /// <summary>
    /// Store the reference to this manager to be accessible everywhere
    /// </summary>
    /// <typeparam name="T">Manager type, implements IManager</typeparam>
    /// <param name="manager"></param>
    public static void SetManager<T>(T manager) where T : IManager
    {
        if (manager == null) throw new ArgumentNullException(nameof(manager));
        if (_managers == null) _managers = new();
        if (_managers.ContainsKey(typeof(T)) && _managers[typeof(T)] != null)
        {
            Debug.LogWarning("A manager '" + nameof(manager) + "' tried to register on the ReferenceManager, but an instance was already registered.");
        }
        _managers[typeof(T)] = manager;
    }
    /// <summary>
    /// Remove the manager associated to this type from the global reference index.
    /// This will NOT break or update existing references that other scripts might hold to this manager.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void RemoveManager<T>() where T : IManager
    {
        if (_managers == null) return;
        if (!(_managers.ContainsKey(typeof(T)) && _managers[typeof(T)] != null))
        {
            Debug.LogWarning("A manager '" + typeof(T).FullName + "' tried to unregister on the ReferenceManager, but no instance was already registered.");
        }
        _managers[typeof(T)] = null;
    }
    /// <summary>
    /// Remove the manager associated to this type from the global reference index.
    /// This will NOT break or update existing references that other scripts might hold to this manager.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void RemoveManager<T>(T manager) where T : IManager => RemoveManager<T>();
    /// <summary>
    /// Removes all the managers from the global reference index.
    /// This will NOT break or update existing references that other scripts might hold to these managers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void RemoveAllManagers()
    {
        if (_managers != null)
            _managers.Clear();
        else
            _managers = new();
    }
    /// <summary>
    /// Get a reference to a manager that registered to the reference index
    /// </summary>
    /// <typeparam name="T">Type of the requested manager</typeparam>
    /// <returns></returns>
    public static T GetManager<T>() where T : class, IManager
    {
        if (_managers == null) return default;
        if (!_managers.ContainsKey(typeof(T)) || _managers[typeof(T)] == null)
        {
            Debug.LogWarning("No manager of type " + typeof(T).FullName + " was found. Key contained : " + (_managers.ContainsKey(typeof(T)) ? "true" : "false"));
            return default;
        }
        return _managers[typeof(T)] as T;
    }
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
    public static bool TryGetManager<T>(out T manager) where T : class, IManager
    {
        manager = GetManager<T>();
        return manager != null;
    }

    #endregion
}
