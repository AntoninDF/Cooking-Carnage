using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Features
{
    /// <summary>
    /// The multi slot container contains slots, that hold items themselves. The multi slot container is useful to keep track of the content of a fixed set of items.
    /// </summary>
    public class MultiSlotContainer : Container, IFeatureCollection
    {
        #region Properties

        public MultiContainer Container { get; set; }
        public List<SoloContainer> SoloContainers { get; set; } = new();
        #endregion


        private Action<Grabbable> Grab => (item) => RequestGrab(item);
        private Action<Grabbable> Drop => (item) => RequestDrop(item);



        #region Methods
        public MultiSlotContainer(IFeatureCollection owner) : base(owner)
        {
        }
        public MultiSlotContainer(IFeatureCollection owner, Transform transform) : base(owner)
        {
            this.transform = transform;
        }

        public override void LateSetup()
        {
            base.LateSetup();
            Container = new(this);
            this.Container.Filter.AddCondition(item => CanGrab(item as Grabbable));
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
        public bool TryAddSlot(SoloContainer slot)
        {
            if (SoloContainers.Contains(slot)) return false;
            SoloContainers.Add(slot);
            TryBind(slot.OnGrab, Grab);
            TryBind(slot.OnDrop, Drop);
            return true;
        }
        public bool TryRemoveSlot(SoloContainer slot)
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
        public override bool RequestGrab(Grabbable item) => TryGrab(item);

        private bool TryGrab(Grabbable item)
        {
            if (!CanGrab(item)) return false;
            GrabByFirstAvailableSlot(item);
            return true;
        }

        public override bool CanGrab(Grabbable grabbable)
        {
            if (!base.CanGrab(grabbable)) return false;
            var slot = GetFirstAvailableSlot(grabbable);
            if (slot == null) return false;

            return true;
        }

        

        public override bool RequestDrop(Grabbable item = null) => TryDrop(item);
        private bool TryDrop(Grabbable item)
        {
            if (!CanDrop(item)) return false;
            DropByFirstAvailableSlot(item);
            return true;
        }
        public override bool CanDrop(Grabbable item = null)
        {
            if (!base.CanDrop(item)) return false;
            if (!Container.CanDrop(item)) return false;

            return true;
        }
        private SoloContainer GetFirstAvailableSlot(Grabbable grabbable)
        {
            foreach (var slot in SoloContainers)
            {
                if (slot.CanGrab(grabbable))
                    return slot;
            }
            return null;
        }
        private void DropByFirstAvailableSlot(Grabbable grabbable)
        {
            var slot = GetFirstAvailableSlot(grabbable);
            if (slot != null)
            {
                if (slot.RequestDrop(grabbable))
                    OnDrop?.Invoke(grabbable);
            }
        }

        private void GrabByFirstAvailableSlot(Grabbable grabbable)
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

        #endregion
    }

}