using System;

/// <summary>
/// Param<typeparamref name="T"/> is just a wrapper for <typeparamref name="T"/> that extends it with OnValueChange, so that the code can react to parameters being changed.
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class Param<T>
{
    public Param(T value) => Value = value;
    public T Value;
    public Action<T> OnValueChange;
    public static implicit operator T(Param<T> obj) => obj.Value;
    public static implicit operator Param<T>(T obj) => new(obj);
}