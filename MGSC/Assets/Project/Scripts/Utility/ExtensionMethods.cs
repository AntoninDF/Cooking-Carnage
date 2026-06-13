using Features;
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