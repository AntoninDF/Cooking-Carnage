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
    /// Checks if the component exists on the gameObject. If so, it is stored in comp
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

    public static string ToString_Detailed<T1, T2>(this Dictionary<T1, T2> dict)
    {
        if (dict == null) return "null";
        string result = "Dictionary<" + typeof(T1).Name + ", " + typeof(T2).Name + "> (Count : " + dict.Count + ")";

        foreach (var kvp in dict)
        {
            result += "\n\t";

            if (kvp.Key is IFormattable key)
                result += key.ToString();
            if (kvp.Key is object obj1)
                result += obj1.ToString();
            else
                result += typeof(T1).Name;

            result += " : ";

            if (kvp.Value is IFormattable value)
                result += value.ToString();
            if (kvp.Value is object obj2)
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
            if (elm is object obj)
                result += obj.ToString();
            else
                result += typeof(T).Name;
        }

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