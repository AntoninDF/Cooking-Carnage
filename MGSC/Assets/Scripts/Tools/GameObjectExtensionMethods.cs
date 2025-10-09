using UnityEngine;

public static class GameObjectExtensionMethods
{
    public static bool HasComponent<T>( this GameObject gameObject ) where T : Component
    {
        return gameObject.GetComponent<T>() != null;
    }
}
