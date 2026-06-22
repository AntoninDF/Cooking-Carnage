using System.Collections.Generic;
using UnityEngine;

namespace Features
{
    public class F_MultiContainer : F_Container
    {
        public List<F_Grabbable> HeldItems { get; set; } = new();
        public override F_Grabbable HeldItem => HeldItems.Count > 0 ? HeldItems[0] : null;

        public F_MultiContainer(IFeatureCollection owner) : base(owner)
        {
        }
        public F_MultiContainer(IFeatureCollection owner, Transform transform) : base(owner, transform)
        {
            this.transform = transform;
        }

        public override void Empty()
        {
            foreach (var item in HeldItems)
            {
                item.Container = null;
            }
            HeldItems = new();
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
            HeldItems.Add(item);
            item.Container?.RequestDrop(item);
            item.GetGrabbedBy(this);
            OnGrab?.Invoke(item);
        }
        protected virtual void Drop(F_Grabbable item = null)
        {
            HeldItems.Remove(item);
            OnDrop?.Invoke(item);
        }
        public override bool CanGrab(F_Grabbable item)
        {
            if (!base.CanGrab(item)) return false;
            if (HeldItems.Contains(item)) return false;

            return true;
        }
        public override bool CanDrop(F_Grabbable item = null)
        {
            if (!HeldItems.Contains(item)) return false;
            return base.CanDrop(item);
        }

        public List<T> GetHeldItemsAs<T>() where T : Feature
        {
            List<T> result = new List<T>();
            foreach(var heldItem in HeldItems)
            {
                if (heldItem.TryGetSibling(out T item))
                    { result.Add(item); }
            }
            return result;
        }
        public bool TryGetHeldItemsAs<T>(out List<T> items) where T : Feature
        {
            items = GetHeldItemsAs<T>();
            if (items == null) return false;
            return true;
        }
    }
}