
using Cooking;
using System;
using System.Transactions;
using UnityEngine;
using UnityEngine.Events;

public abstract class Container : FeatureComponent, IFeatureCollection
{

    /// <summary>
    /// A locked container doesn't allow movements in it's content
    /// </summary>
    public virtual bool IsLocked { get; set; }
    /// <summary>
    /// A passthrough container can be directly interacted with by the player to get the contained item, rather than being accessed through it's contained item.
    /// </summary>
    public virtual bool Passthrough { get; set; }
    /// <summary>
    /// Returns the first/default/only object contained by the container, or null if there is none.
    /// </summary>
    public virtual Grabbable HeldItem { get; protected set; }
    public Transform transform {  get; set; }
    public ContainerFilter Filter { get; set; }

    public FeatureCollection Collection { get; set; } = new();

    public Action<Grabbable> OnGrab;
    public Action<Grabbable> OnDrop;

    protected Container(IFeatureCollection owner) : base(owner)
    {
        Filter = new(this);
    }
    protected Container(IFeatureCollection owner, Transform transform) : base(owner)
    {
        this.transform = transform;
    }

    public abstract bool RequestGrab(Grabbable item);
    public abstract bool RequestDrop(Grabbable item = null);
    public abstract void Empty();
    public virtual bool CanGrab(Grabbable item)
    {
        if (!Enabled) return false;
        UnityEngine.Debug.Log("1");
        if (item == null) return false;
        UnityEngine.Debug.Log("2");
        if (IsLocked) return false;
        UnityEngine.Debug.Log("3");
        if (item.IsLocked) return false;
        UnityEngine.Debug.Log("4");
        if (item.Container  != null && item.Container.IsLocked) return false;
        UnityEngine.Debug.Log("5");
        if (Filter != null && !Filter.IsValid(item)) return false;
        UnityEngine.Debug.Log("6");

        return true;
    }
    public virtual bool CanDrop(Grabbable item = null)
    {
        if (!Enabled) return false;
        if (IsLocked) return false;
        if (item != null && item.IsLocked) return false;
        if (item != null && item.Container != null && item.Container.IsLocked) return false;

        return true;
    }
}
