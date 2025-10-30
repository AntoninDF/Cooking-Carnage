using UnityEditor;
using UnityEngine;

/// <summary>
/// Helper methods to extend functionality to GameObjects
/// </summary>
public static class GameObjectExtensionMethods
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
}