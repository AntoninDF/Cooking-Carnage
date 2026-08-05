using Features;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper methods to extend functionality to GameObjects
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// Checks if the component exists on the gameObject
    /// </summary>
    /// <typeparam name="T">Component</typeparam>
    /// <param name="gameObject"></param>
    /// <returns>True if the GameObject has the specified component, false otherwise</returns>
    public static bool HasComponent<T>( this GameObject gameObject ) where T : Component
    {
        return gameObject.GetComponent<T>() != null;
    }

    /// <summary>
    /// Checks if the component exists on the gameObject. If so, it is stored in go
    /// </summary>
    /// <typeparam name="T">Component</typeparam>
    /// <param name="gameObject"></param>
    /// <returns>True if the GameObject has the specified component, false otherwise</returns>
    public static bool HasComponent<T>(this GameObject gameObject, ref T comp) where T : Component
    {
        T temp = gameObject.GetComponent<T>();
        bool valid = gameObject.GetComponent<T>() != null;
        if (valid) comp = temp;
        return valid;
    }
    /// <summary>
    /// Scans the hierarchy of the GameObject and returns the first object with a matching name and type.
    /// The type must either be <see cref="GameObject"/>, <see cref="Transform"/> or derive from <see cref="Component"/>.
    /// Can return itself if it matches.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go">The root caller.</param>
    /// <param name="name">The name of the gameObject (or owning gameObject) to look for</param>
    /// <returns></returns>
    public static T FindAnyObjectByName<T>(this GameObject go, string name) where T : UnityEngine.Object
    {
        if (go.name == name)
        {
            if (typeof(T) == typeof(Transform))
                return go.transform as T;
            if (typeof(T) == typeof(GameObject))
                return go as T;
            if (go.TryGetComponent(out T result))
                return result;
        }
        for (int i = 0; i < go.transform.childCount; i++)
        {
            T result = go.transform.GetChild(i).gameObject.FindAnyObjectByName<T>(name);
            if (result != null)
                return result;
        }
        return null;
    }
    /// <summary>
    /// Scans the hierarchy of the GameObject and returns the first object with a matching name and type.
    /// Can return itself if it matches.
    /// You can safely cast the returned object to <paramref name="type"/> after checking it's not null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go">The root caller.</param>
    /// <param name="name">The name of the gameObject (or owning gameObject) to look for</param>
    /// <param name="type">The type must either be <see cref="GameObject"/>, <see cref="Transform"/> or derive from <see cref="Component"/>.</param>
    /// <returns></returns>
    public static UnityEngine.Object FindAnyObjectByName(this GameObject go, string name, Type type, Func<string, string, bool> comparer=null)
    {
        if (type != typeof(GameObject) && type != typeof(Transform) && !type.IsSubclassOf(typeof(Component)))
            return null;
        if (go.name == name || (comparer != null && comparer.Invoke(go.name, name)))
        {
            if (type == typeof(Transform))
                return go.transform;
            if (type == typeof(GameObject))
                return go;
            if (go.TryGetComponent(type, out Component result))
                return result;
        }
        for (int i = 0; i < go.transform.childCount; i++)
        {
            var result = go.transform.GetChild(i).gameObject.FindAnyObjectByName(name, type, comparer);
            if (result != null)
                return result;
        }
        return null;
    }

    public static string ToString_Detailed<T1, T2>(this Dictionary<T1, T2> dict)
    {
        if (dict == null) return "null";
        string result = "Dictionary<" + typeof(T1).Name + ", " + typeof(T2).Name + "> (Count : " + dict.Count + ")";

        foreach (var kvp in dict)
        {
            result += "\n\t";

            if (kvp.Key is IFormattable key)
                result += key.ToString();
            else if (kvp.Key is object obj1)
                result += obj1.ToString();
            else
                result += typeof(T1).Name;

            result += " : ";

            if (kvp.Value is IFormattable value)
                result += value.ToString();
            else if (kvp.Value is object obj2)
                result += obj2.ToString();
            else
                result += typeof(T2).Name;
        }

        return result;
    }
    public static string ToString_Detailed<T>(this List<T> list)
    {
        if (list == null) return "null";
        string result = "List<" + typeof(T).Name + "> (Count : " + list.Count + ")";

        foreach (var elm in list)
        {
            result += "\n\t";

            if (elm is IFormattable formattable)
                result += formattable.ToString();
            else if (elm is object obj)
                result += obj.ToString();
            else
                result += typeof(T).Name;
        }

        return result;
    }
    public static string ToString_EasyRead<T>(this List<T> list, string startWith="(", string separator=", ", string endWith=")")
    {
        if (list == null) return "null";
        if (list.Count == 0) return "(empty list)";
        string result = startWith;

        foreach (var elm in list)
        {
            if (elm is IFormattable formattable)
                result += formattable.ToString();
            if (elm is object obj)
                result += obj.ToString();
            else
                result += typeof(T).Name;
            result += separator;
        }
        result = result.Substring(0, result.Length - 2);
        result += endWith;

        return result;
    }

    public static T GetFeature<T>(this object obj) where T : Features.Feature
    {
        if (obj is IFeatureCollection col)
        {
            return col.Get<T>();
        }
        return null;
    }
    public static bool HasFeature<T>(this object obj) where T : Features.Feature
    {
        T feature;
        if (obj is IFeatureCollection col)
        {
            feature = col.Get<T>();
            if (feature != null) return true;
        }
        return false;
    }
    public static bool TryGetFeature<T>(this object obj, out T feature, bool mustBeEnabled=false) where T : Features.Feature
    {
        feature = null;
        if (obj is IFeatureCollection col)
        {
            feature = col.Get<T>(mustBeEnabled);
            if (feature != null) return true;
        }
        return false;
    }
}