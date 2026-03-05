using Features;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EventAsset", menuName = "Scriptable Objects/Event Asset")]
public class EventAsset : ScriptableObject
{
    public Action OnInvoke;
    public Action<EventContext> OnContextualInvoke;

    public void Invoke() => OnInvoke?.Invoke();
    public void Invoke(int value) => OnContextualInvoke?.Invoke(new EventContext(value));
    public void Invoke(string value) => OnContextualInvoke?.Invoke(new EventContext(value));
    public void Invoke(float value) => OnContextualInvoke?.Invoke(new EventContext(value));
    public void Invoke(bool value) => OnContextualInvoke?.Invoke(new EventContext(value));
    public void Invoke(UnityEngine.Object value) => OnContextualInvoke?.Invoke(new EventContext(value));

}
