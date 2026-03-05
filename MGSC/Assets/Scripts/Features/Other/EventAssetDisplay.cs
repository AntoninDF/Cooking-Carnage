using UnityEngine;
using UnityEngine.Events;

[HideScriptField]
public class EventAssetDisplay : MonoBehaviour
{
    public EventAsset Target;
    public UnityEvent OnInvoke;

    private void Awake()
    {
        if (Target == null) return;
        Target.OnInvoke += OnInvoke.Invoke;
    }
}