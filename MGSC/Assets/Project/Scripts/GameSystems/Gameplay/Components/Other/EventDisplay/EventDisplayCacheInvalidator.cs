#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public static class EventDisplayCacheInvalidator
{
    static EventDisplayCacheInvalidator()
    {
        EventDisplay.BuildGlobalActionCache(); 
    }
}
#endif
