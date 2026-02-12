using Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// The multi slot container contains slots, that hold items themselves. The multi slot container is useful to keep track of the content of a fixed set of items.
    /// </summary>
    public class MultiSlotContainer : MonoBehaviour, IMultiContainer, ICallbackBroadcaster
    {
        #region Properties
        public List<IGrabbable> HeldItems => _items;
        public IGrabbable HeldItem => _items == null ? null : _items.Count > 0 ? _items[_items.Count-1] : null;
        public UnityEvent<IGrabbable> OnGrab => _onGrab;
        public UnityEvent<IGrabbable> OnDrop => _onDrop;
        public PlayerInterface Player { get; set; }
        public List<SlotContainer> Slots => _slots;
        public bool IsLocked { get; set; }
        public bool Passthrough => false;

        public bool EnableCallbacks { get; set; }

        public ICallbackBroadcaster Self => this;

        #endregion

        #region Fields
        [Header("Multislot references")]
        [Tooltip("List of all the slots that belong to this multi-slot container")]
        [SerializeField] private List<SlotContainer> _slots = new ();
        [Tooltip("Wether or not this container should scan it's hierarchy on Start to find it's own slots")]
        [SerializeField] private bool _detectSlots;
        List<IGrabbable> _items = new();  // List of all the items contained in the slots.
        [Space]
        [Header("Multislot events")]
        [Tooltip("Invoked when an item is put in one of the slots")]
        [SerializeField] private UnityEvent<IGrabbable> _onGrab;
        [Tooltip("Invoked when an item is taken out of one of the slots")]
        [SerializeField] private UnityEvent<IGrabbable> _onDrop;


        private ContainerFilter _filter;
        #endregion

        #region Methods

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            if (_slots.Count == 0)
            {
                if (!_detectSlots)
                    Debug.LogWarning("Please reference the slots for this multi-slot container in the inspector, or opt for automatic detection. (GameObject : " + name + ")");
                else
                {
                    throw new NotImplementedException("The automatic detection of child slots isn't yet implemented. If it is a problem, warn me :) (GameObject : " + name + ")");
                }
            }

            foreach (var slot in _slots)
            {
                slot.OnGrab.AddListener(RecordGrab);
                slot.OnDrop.AddListener(RecordDrop);
            }
        }

        public void RecordDrop(IGrabbable item)
        {
            if (!_items.Contains(item)) return;
            _items.Remove(item);
            Self.Callback(OnDrop, item);
        }
        public void RecordGrab(IGrabbable item)
        {
            if (_items.Contains(item)) return;
            _items.Add(item);
            Self.Callback(OnGrab, item);
        }
        public void Empty()
        {
            foreach(var slot in _slots)
                slot.Empty();
        }
        public void SetActive(bool active)
        {
            IsLocked = !active;
            foreach (var slot in _slots)
                slot.IsLocked = !active;
        }
        public SlotContainer GetFirstAvailableSlot()
        {
            foreach(SlotContainer slot in _slots)
            {
                if (slot.IsFree && !slot.IsLocked) return slot;
            }
            return null;
        }
        public SlotContainer GetFirstAvailableSlot(IGrabbable item)
        {
            foreach(SlotContainer slot in _slots)
            {
                if (slot.CanGrab(item)) return slot;
            }
            return null;
        }

        public void Reset()
        {
            Player = null;
            _items = new();
            Empty();
        }

        public bool RequestGrab(IGrabbable item)
        {
            return TryGrab(item);
        }

        public bool RequestDrop(IGrabbable item = null)
        {
            return TryDrop(item);
        }
        protected bool TryGrab(IGrabbable item)
        {
            if (!CanGrab(item))
                return false;
            Grab(item);
            return true;
        }
        protected bool TryDrop(IGrabbable item)
        {
            if (!CanDrop(item)) return false;
            Drop(item);
            return true;
        }
        protected void Grab(IGrabbable item)
        {
            var slot = GetFirstAvailableSlot(item);
            slot.RequestGrab(item);
            RecordGrab(item);
            Self.Callback(OnGrab, item);
        }
        protected void Drop(IGrabbable item)
        {
            item.Container.RequestDrop(item);
            RecordDrop(item);
            Self.Callback(OnDrop, item);
        }
        public bool CanGrab(IGrabbable item)
        {
            if (item == null) return false;
            if (!_slots.Any((slot) => slot.CanGrab(item))) return false;
            if (IsLocked) return false;
            if (item.Container != null)
                if (!item.Container.CanDrop(item))
                    return false;
            return true;
        }
        public bool CanDrop(IGrabbable item)
        {
            if (item == null)           return false;
            if (item.Container == null) return false;
            if (IsLocked)               return false;
            if (item.IsLocked)          return false;
            if (item.Container.IsLocked) return false;
            if (item.Container != this as IContainer && !item.Container.CanDrop(item)) return false;
            return true;
        }
        #endregion
    }

}