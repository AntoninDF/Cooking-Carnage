using System;
using UnityEngine;

namespace Features
{
    public abstract class F_Container : Feature, IFeatureCollection
    {

        /// <summary>
        /// A locked container doesn't allow movements in it's content
        /// </summary>
        public virtual bool IsLocked { get; set; }
        /// <summary>
        /// Returns the first/default/only object contained by the container, or null if there is none.
        /// </summary>
        public virtual Grabbable HeldItem { get; protected set; }
        public Transform transform { get; set; }
        public Conditions Filter { get; set; }

        public FeatureCollection Collection { get; set; } = new();

        public Action<Grabbable> OnGrab;
        public Action<Grabbable> OnDrop;

        protected F_Container(IFeatureCollection owner) : base(owner)
        {
            Filter = new(this);
            SetupFilter();
            transform = GetParentTransform();
        }
        protected F_Container(IFeatureCollection owner, Transform transform) : base(owner)
        {
            Filter = new(this);
            SetupFilter();
            this.transform = transform;
        }

        private void SetupFilter()
        {
            Filter.AddCondition(item => item is Grabbable);
        }


        public abstract bool RequestGrab(Grabbable item);
        public abstract bool RequestDrop(Grabbable item = null);
        public abstract void Empty();
        public virtual bool CanGrab(Grabbable item)
        {
            if (!Enabled) return false;
            if (item == null) return false;
            if (IsLocked) return false;
            if (item.IsLocked) return false;
            if (item.Container != null && item.Container.IsLocked) return false;
            if (Filter != null && !Filter.IsValid(item)) return false;

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
}