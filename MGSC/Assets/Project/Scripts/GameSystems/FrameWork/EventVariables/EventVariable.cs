using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "EventVariable", menuName = "Scriptable Objects/EventVariable")]
public class EventVariable : ScriptableObject
{
    private event Action _event;
    public object Parameter;

    public void AddListener(Action callback)
    {
        if (_event != null && _event.GetInvocationList().Contains(callback))
            return;
        _event += callback;
    }
    public void RemoveListener(Action callback)
    {
        _event -= callback;
    }

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
