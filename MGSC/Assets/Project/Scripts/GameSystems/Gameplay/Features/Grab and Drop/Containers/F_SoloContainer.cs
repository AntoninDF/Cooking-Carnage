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


        public override bool RequestGrab(F_Grabbable item) => TryGrab(item);

        public override bool RequestDrop(F_Grabbable item = null) => TryDrop(item);
        protected bool TryGrab(F_Grabbable item)
        {
            if (!CanGrab(item))
                return false;
            Grab(item);
            return true;
        }
        protected bool TryDrop(F_Grabbable item = null)
        {
            if (!CanDrop(item))
                return false;
            Drop(item);
            return true;
        }
        protected virtual void Grab(F_Grabbable item)
        {
            HeldItem = item;
            item.Container?.RequestDrop(item);
            item.GetGrabbedBy(this);
            OnGrab?.Invoke(item);
//            Debug.Log($"[F_SoloContainer] Grab réussi sur transform '{transform?.name}', instance hash {GetHashCode()}");

        }
        protected virtual void Drop(F_Grabbable item = null)
        {
            var temp = HeldItem;
            HeldItem.Container = null;
            HeldItem = null;
            OnDrop?.Invoke(temp);
        }
        public override bool CanGrab(F_Grabbable item)
        {
            if (item == HeldItem) return false;
            if (HeldItem != null) return false;
            return base.CanGrab(item);
        }
        public override bool CanDrop(F_Grabbable item = null)
        {
            if (item != null && item != HeldItem) return false;
            if (item == null && HeldItem == null) return false;
            return base.CanDrop(item);
        }
    }
}