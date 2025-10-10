using Cooking;
using System;
using UnityEngine;

public interface IContainer
{
    public Transform transform { get; } // Lowercase to ensure ease of use
    public void Grab(IGrabbable item);
    public void Drop(IGrabbable item);
    public Action<IGrabbable> OnGrab { get; set; }
    public Action<IGrabbable> OnDrop { get; set; }
}
