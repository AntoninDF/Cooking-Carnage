using System;

/// <summary>
/// Globals is a static class that contains many various parameters allowing control over the game's flow, rendering, etc.
/// </summary>
public static partial class Globals
{
    public static Action OnRefresh;
    public static void Refresh() => OnRefresh?.Invoke();

    /// <summary>
    /// Param<typeparamref name="T"/> is just a wrapper for <typeparamref name="T"/> that extends it with OnValueChange, so that the code can react to parameters being changed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Param<T>
    {
        public Param(T value)
        {
            Value = value;
            Globals.OnRefresh += () => OnValueChange?.Invoke(Value);
        }
        public T Value;
        public Action<T> OnValueChange;
        public static implicit operator T (Param<T> obj) => obj.Value;
        public static implicit operator Param<T> (T obj) => new(obj);

    }
}
