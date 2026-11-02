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
        public virtual F_Grabbable HeldItem { get; protected set; }
        public Transform transform { get; set; }
        public Conditions Filter { get; set; }
        public bool AllowItemPlacedOnSurfaceRatherThanTransform;

        public FeatureCollection Collection { get; set; } = new();

        public Action<F_Grabbable> OnGrab;
        public Action<F_Grabbable> OnDrop;

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
            Filter.AddCondition(item => item is F_Grabbable);
        }


        public abstract bool RequestGrab(F_Grabbable item);
        public abstract bool RequestDrop(F_Grabbable item = null);
        /// <summary>
        /// Empty() will simply remove all references to the container's content, but will not explicitely 
        /// destroy any contained objects
        /// </summary>
        public abstract void Empty();
        public virtual bool CanGrab(F_Grabbable item)
        {
            if (!Enabled) return false;
            if (item == null) return false;
            if (IsLocked) return false;
            if (item.IsLocked) return false;
            if (item.Container != null && item.Container.IsLocked) return false;
            if (Filter != null && !Filter.IsValid(item)) return false;

            return true;
        }
        public virtual bool CanDrop(F_Grabbable item = null)
        {
            Debug.Log("1, container is " + Owner);
            if (!Enabled) return false;
            Debug.Log("2, container is " + Owner);
            if (IsLocked) return false;
            Debug.Log("3, container is " + Owner);
            if (item != null && item.IsLocked) return false;
            Debug.Log("4, container is " + Owner);
            if (item != null && item.Container != null && item.Container.IsLocked) return false;
            Debug.Log("5, container is " + Owner);

            return true;
        }
    }
}