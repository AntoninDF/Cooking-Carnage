using Cooking;
using Unity.VisualScripting;
using UnityEngine;

public class SoloContainer : Container
{
    public SoloContainer(IFeatureCollection owner) : base(owner)
    {
    }
    public SoloContainer(IFeatureCollection owner, Transform transform) : base(owner)
    {
        this.transform = transform;
    }

    public override void Empty()
    {
        if (HeldItem == null) return;
        HeldItem.Container = null;
        HeldItem = null;
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
        HeldItem = item;
        item.Container?.RequestDrop(item);
        item.GetGrabbedBy(this);
        OnGrab?.Invoke(item);
    }
    protected virtual void Drop(Grabbable item = null)
    {
        OnDrop?.Invoke(HeldItem);
        HeldItem = null;
    }
    public override bool CanGrab(Grabbable item)
    {
        if (item == HeldItem) return false;
        if (HeldItem != null) return false;
        return base.CanGrab(item);
    }
}
