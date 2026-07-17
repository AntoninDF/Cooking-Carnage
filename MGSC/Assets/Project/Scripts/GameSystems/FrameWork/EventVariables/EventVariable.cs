using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EventVariable", menuName = "Scriptable Objects/EventVariable")]
public class EventVariable : ScriptableObject
{
    private event Action _event;
    public object Parameter;

    public void Subscribe(Action callback)
        => _event += callback;
    public void Unsubscribe(Action callback)
        => _event -= callback;
    public void Raise(object optionalParameter)
    {
        Parameter = optionalParameter;
        _event?.Invoke();
    }
    public void Raise()
    {
        Parameter = null;
        _event?.Invoke();
    }
}
