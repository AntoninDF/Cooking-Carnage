using Cooking;
using System;
using UnityEngine;

public interface IContainer : IGeneric<IContainer>
{
    public void Grab(IGrabbable item);
    public void Drop(IGrabbable item);
    public Action<IGrabbable> OnGrab { get; set; }
    public Action<IGrabbable> OnDrop { get; set; }
}
