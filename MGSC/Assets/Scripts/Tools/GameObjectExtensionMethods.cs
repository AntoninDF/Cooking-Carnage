using UnityEngine;

/// <summary>
/// Helper methods to extend functionality to GameObjects
/// </summary>
public static class GameObjectExtensionMethods
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Component</typeparam>
    /// <param name="gameObject"></param>
    /// <returns>True if the GameObject has the specified component, false otherwise</returns>
    public static bool HasComponent<T>( this GameObject gameObject ) where T : Component
    {
        return gameObject.GetComponent<T>() != null;
    }

}