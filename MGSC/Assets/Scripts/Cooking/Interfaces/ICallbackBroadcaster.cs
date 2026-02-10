

using System;
using UnityEngine.Events;

public interface ICallbackBroadcaster
{
    public bool EnableCallbacks { get; set; }
    public ICallbackBroadcaster Self { get; }
    public bool Callback(Action action)
    {
        if (EnableCallbacks)
            action?.Invoke();
        return EnableCallbacks;
    }
    public bool Callback(UnityEvent action)
    {
        if (EnableCallbacks)
            action?.Invoke();
        return EnableCallbacks;
    }
    public bool Callback<T>(Action<T> action, T arg)
    {
        if (EnableCallbacks)
            action?.Invoke(arg);
        return EnableCallbacks;
    }
    public bool Callback<T>(UnityEvent<T> action, T arg)
    {
        if (EnableCallbacks)
            action?.Invoke(arg);
        return EnableCallbacks;
    }
    public bool Callback<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
    {
        if (EnableCallbacks)
            action?.Invoke(arg1, arg2);
        return EnableCallbacks;
    }
    public bool Callback<T1, T2>(UnityEvent<T1, T2> action, T1 arg1, T2 arg2)
    {
        if (EnableCallbacks)
            action?.Invoke(arg1, arg2);
        return EnableCallbacks;
    }
}