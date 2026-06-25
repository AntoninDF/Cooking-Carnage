using System;

/// <summary>
/// Param<typeparamref name="T"/> is just a wrapper for <typeparamref name="T"/> that extends it with OnValueChange, so that the code can react to parameters being changed.
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class Param<T>
{
    object myobj;
    public Param(T value) => Value = value;
    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            OnValueChange?.Invoke(value);
        }
    }
    private T _value;
    /// <summary>
    /// OnValueChange is invoked everytime the value is set. It is invoked even if the new value equals the previous value. It passes the new value as parameter
    /// </summary>
    public Action<T> OnValueChange { get; set; }
    public static implicit operator T(Param<T> obj) => obj.Value;
    public static implicit operator Param<T>(T obj) => new(obj);
}