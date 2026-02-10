using Cooking;
using System;
using UnityEngine;
using UnityEngine.Events;

public interface IContainer : IGeneric<IContainer>
{
    /// <summary>
    /// Public method for other containers to ask this container to grab the item
    /// </summary>
    /// <param name="item"></param>
    /// <returns>Wether or not the request was accepted</returns>
    public bool RequestGrab(IGrabbable item);
    /// <summary>
    /// Public method for other containers to ask this container to drop the item
    /// </summary>
    /// <param name="item"></param>
    /// <returns>Wether or not the request was accepted</returns>
    public bool RequestDrop(IGrabbable item = null);
    public void Empty();
    public UnityEvent<IGrabbable> OnGrab { get; }
    public UnityEvent<IGrabbable> OnDrop { get; }
    public bool CanGrab(IGrabbable item);
    public bool CanDrop(IGrabbable item = null);
    /// <summary>
    /// A locked container doesn't allow movements in it's content
    /// </summary>
    public bool IsLocked { get; }
    /// <summary>
    /// A passthrough container can be directly interacted with by the player to get the contained item, rather than being accessed through it's contained item.
    /// </summary>
    public bool Passthrough { get; }
    /// <summary>
    /// Returns the first/default/only object contained by the container, or null if there is none.
    /// </summary>
    public IGrabbable HeldItem { get; }
}
