using System;
using System.ComponentModel;

/// <summary>
/// Globals is a static class that contains many various parameters allowing control over the game's flow, rendering, etc.
/// </summary>
public static partial class Globals
{
    // Empty because the content is mostly define in other partial scripts (rendering, gameplay etc..)



    /// <summary>
    /// Utility method that takes in a setter, applies the parameter value to it, and binds it so that further parameter changes are reflected on the setter.
    /// </summary>
    /// <typeparam name="T">The type for the property and parameter to bind together</typeparam>
    /// <param name="setter">Action that takes in a value and applies it to a target field or property. It typically looks like value => MyProperty = value</param>
    /// <param name="parameter">The global parameter that should be reflected upon the setter's field or property</param>
    public static void Bind<T>(Action<T> setter, Param<T> parameter)
    {
        setter?.Invoke(parameter);
        parameter.OnValueChange += setter;
    }
}
