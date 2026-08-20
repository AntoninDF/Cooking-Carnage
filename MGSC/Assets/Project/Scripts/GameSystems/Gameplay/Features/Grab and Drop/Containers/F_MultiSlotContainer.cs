using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Features
{
    /// <summary>
    /// The multi slot container contains slots, that hold items themselves. The multi slot container is useful to keep track of the content of a fixed set of items.
    /// </summary>
    public class F_MultiSlotContainer : F_Container, IFeatureCollection
    {
        #region Properties

        public F_MultiContainer Container { get; set; }
        public List<F_SoloContainer> SoloContainers { get; set; } = new();
        #endregion


        private Action<F_Grabbable> Grab => (item) => RequestGrab(item);
        private Action<F_Grabbable> Drop => (item) => RequestDrop(item);



        #region Methods
        public F_MultiSlotContainer(IFeatureCollection owner) : base(owner)
        {
        }
        public F_MultiSlotContainer(IFeatureCollection owner, Transform transform) : base(owner)
        {
            this.transform = transform;
        }

        public override void LateSetup()
        {
            base.LateSetup();
            Container = new(this);
            this.Container.Filter.AddCondition(item => CanGrab(item as F_Grabbable));
            SetupSlots();
        }

        private void SetupSlots()
        {
            foreach (var slot in SoloContainers)
            {
                TryBind(slot.OnGrab, Grab);
                TryBind(slot.OnDrop, Drop);
            }
        }
        /// <summary>
        /// Adds a slot to the list of tracked slots and binds it to this MultiSlotContainer
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public bool TryAddSlot(F_SoloContainer slot)
        {
            if (SoloContainers.Contains(slot)) return false;
            SoloContainers.Add(slot);
            TryBind(slot.OnGrab, Grab);
            TryBind(slot.OnDrop, Drop);
            return true;
        }
        public bool TryRemoveSlot(F_SoloContainer slot)
        {
            if (!SoloContainers.Contains(slot)) return false;
            SoloContainers.Remove(slot);
            TryUnbind(slot.OnGrab, Grab);
            TryUnbind(slot.OnDrop, Drop);
            return true;
        }
        private bool TryBind<T>(Action<T> action, Action<T> invocation)
        {
            if (action != null && action.GetInvocationList().Contains(invocation)) return false;

            action += invocation;
            return true;
        }
        private bool TryUnbind<T>(Action<T> action, Action<T> invocation)
        {
            if (action != null && !action.GetInvocationList().Contains(invocation)) return false;

            action -= invocation;
            return true;
        }
        public override bool RequestGrab(F_Grabbable item) => TryGrab(item);

        private bool TryGrab(F_Grabbable item)
        {
            if (!CanGrab(item)) return false;
            GrabByFirstAvailableSlot(item);
            return true;
        }

        public override bool CanGrab(F_Grabbable grabbable)
        {
            if (!base.CanGrab(grabbable)) return false;
            var slot = GetFirstAvailableSlot(grabbable);
            if (slot == null) return false;

            return true;
        }

        

        public override bool RequestDrop(F_Grabbable item = null) => TryDrop(item);
        private bool TryDrop(F_Grabbable item)
        {
            if (!CanDrop(item)) return false;
            DropByFirstAvailableSlot(item);
            return true;
        }
        public override bool CanDrop(F_Grabbable item = null)
        {
            if (!base.CanDrop(item)) return false;
            if (!Container.CanDrop(item)) return false;

            return true;
        }
        private F_SoloContainer GetFirstAvailableSlot(F_Grabbable grabbable)
        {
            F_SoloContainer fallback = null;
            foreach (var slot in SoloContainers)
            {
                if (slot.CanGrab(grabbable))
                {
                    if (fallback == null) fallback = slot;
                }
            }
            return fallback;
        }
        private void DropByFirstAvailableSlot(F_Grabbable grabbable)
        {
            var slot = GetFirstAvailableSlot(grabbable);
            if (slot != null)
            {
                if (slot.RequestDrop(grabbable))
                    OnDrop?.Invoke(grabbable);
            }
        }

        private void GrabByFirstAvailableSlot(F_Grabbable grabbable)
        {
            var slot = GetFirstAvailableSlot(grabbable);
            if (slot != null)
            {
                if (slot.RequestGrab(grabbable))
                    OnGrab?.Invoke(grabbable);
            }
        }

        public override void Empty()
        {
            foreach(var slot in SoloContainers)
            {
                slot.Empty();
            }
        }

        public override F_Grabbable HeldItem =>
            SoloContainers.FirstOrDefault(s => s.HeldItem != null)?.HeldItem;
        
        #endregion
    }

}