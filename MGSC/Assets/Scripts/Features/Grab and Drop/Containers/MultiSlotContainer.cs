using System.Collections.Generic;

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

        public MultiSlotContainer(IFeatureCollection owner) : base(owner)
        {
        }

        #region Methods

        public override void Init()
        {
            base.Init();
            Container = new(this);
            this.Container.Filter.AddCondition(CanGrab);
            SetupSlots();
        }

        private void SetupSlots()
        {
            foreach (var slot in SoloContainers)
            {
                slot.OnGrab += (item) => Container.RequestGrab(item);
                slot.OnDrop += (item) => Container.RequestDrop(item);
            }
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