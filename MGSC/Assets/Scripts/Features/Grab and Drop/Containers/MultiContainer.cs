
using FMOD;
using System.Collections.Generic;
using UnityEngine;

public class MultiContainer : Container
{
    public List<Grabbable> HeldItems { get; set; } = new();
    public override Grabbable HeldItem => HeldItems.Count > 0 ? HeldItems[0] : null;

    public MultiContainer(IFeatureCollection owner) : base(owner)
    {
    }
    public MultiContainer(IFeatureCollection owner, Transform transform) : base(owner)
    {
        this.transform = transform;
    }

    public override void Empty()
    {
        foreach(var item in HeldItems)
        {
            item.Container = null;
        }
        HeldItems = new();
    }

    public override bool RequestGrab(Grabbable item) => TryGrab(item);

    public override bool RequestDrop(Grabbable item = null) => TryDrop(item);
    protected bool TryGrab(Grabbable item)
    {
        if (!CanGrab(item))
            return false;
        Grab(item);
        return true;
    }
    protected bool TryDrop(Grabbable item = null)
    {
        if (!CanDrop(item))
            return false;
        Drop(item);
        return true;
    }
    protected virtual void Grab(Grabbable item)
    {
        HeldItems.Add(item);
        item.Container?.RequestDrop(item);
        item.GetGrabbedBy(this);
    }
    protected virtual void Drop(Grabbable item = null)
    {
        HeldItems.Remove(item);
    }
    public override bool CanGrab(Grabbable item)
    {
        if (HeldItems.Contains(item)) return false;
        return base.CanGrab(item);
    }
    public override bool CanDrop(Grabbable item = null)
    {
        if (!HeldItems.Contains(item)) return false;
        return base.CanDrop(item);
    }
}
