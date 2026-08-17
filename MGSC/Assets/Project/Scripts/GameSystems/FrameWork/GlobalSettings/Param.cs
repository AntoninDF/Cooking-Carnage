using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Param<typeparamref name="T"/> is just a wrapper for <typeparamref name="T"/> that extends it with OnValueChange, so that the code can react to parameters being changed.
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class Param<T>
{
    // Constructor
    public Param(T value) => _value = value;

    /// <summary>
    /// OnValueChange is invoked everytime the value is set. It is invoked even if the new value equals the previous value. It passes the new value as parameter
    /// </summary>
    public Action<T> OnValueChange { get; set; }

    // Underlying value
    public T Value
    {
        get => _value;
        set => Set(value);        
    }
    private T _value;

    public void Set(T value)
    {
        if (EqualityComparer<T>.Default.Equals(_value, value))
        {
            _value = value;
            return; 
        }
        _value = value;
        OnValueChange?.Invoke(value);
    }

    public static implicit operator T(Param<T> obj) => obj.Value;
    [Obsolete("Consider using Param.Value = x or Param.Set(x) when reassigning, or Param = new(x) when declaring. Using Param = x would overwrite the subscribers.",true)] public static implicit operator Param<T>(T obj) => new(obj);
    public override string ToString()
    {
        return Value.ToString();
    }
}