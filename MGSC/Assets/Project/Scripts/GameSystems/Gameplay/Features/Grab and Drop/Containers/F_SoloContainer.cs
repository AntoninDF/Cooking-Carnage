using UnityEngine;

namespace Features
{
    public class F_SoloContainer : F_Container
    {
        public F_SoloContainer(IFeatureCollection owner) : base(owner)
        {
            this.transform = GetParentTransform();
        }
        public F_SoloContainer(IFeatureCollection owner, Transform transform) : base(owner, transform)
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
        public override bool CanDrop(Grabbable item = null)
        {
            if (item != null && item != HeldItem) return false;
            if (item == null && HeldItem == null) return false;
            return base.CanDrop(item);
        }
    }
}