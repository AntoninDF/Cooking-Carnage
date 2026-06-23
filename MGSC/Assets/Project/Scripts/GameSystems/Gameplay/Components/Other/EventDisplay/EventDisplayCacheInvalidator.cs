#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class EventDisplayCacheInvalidator
{
    static EventDisplayCacheInvalidator()
    {
        EventDisplay.BuildGlobalActionCache(); 
    }
}
#endif
