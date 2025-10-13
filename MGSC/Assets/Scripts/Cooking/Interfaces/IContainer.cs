using Cooking;
using System;
using UnityEngine;
using UnityEngine.Events;

public interface IContainer : IGeneric<IContainer>
{
    public void Grab(IGrabbable item);
    public void Drop(IGrabbable item);
    public UnityEvent<IGrabbable> OnGrab { get; set; }
    public UnityEvent<IGrabbable> OnDrop { get; set; }
    public bool CanGrab(IGrabbable item);
}
