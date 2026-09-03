using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

[HelpBox("Each event will be invoked on start if the condition is met. " +
    "Demo and Release events are exclusive (it's one or the other). " +
    "Debug is invoked in editor and development builds.")]
[HideScriptField]
public class VersionSpecificBehaviour : MonoBehaviour
{
    public UnityEvent IfReleaseVersion;
    public UnityEvent IfDemoVersion;
    public UnityEvent IfDebugVersion;
    void Start()
    {
#if DEMO
        IfDemoVersion?.Invoke();
#else
        IfReleaseVersion?.Invoke();
#endif
#if DEVELOPMENT_BUILD
        IfDebugVersion?.Invoke();
#endif

    }
}
