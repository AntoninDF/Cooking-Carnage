using Controller;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// The multi slot container contains slots, that hold items themselves. The multi slot container is useful to keep track of the content of a fixed set of items.
    /// </summary>
    public class MultiSlotContainer : MonoBehaviour, IMultiContainer
    {
        #region Properties
        public List<IGrabbable> HeldItems => _items;
        public UnityEvent<IGrabbable> OnGrab => _onGrab;
        public UnityEvent<IGrabbable> OnDrop => _onDrop;
        public PlayerInterface Player { get; set; }
        public List<SlotContainer> Slots => _slots;
        public bool IsLocked => false;

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
                    throw new MissingReferenceException("Please reference the slots for this multi-slot container in the inspector, or opt for automatic detection. (GameObject : " + name + ")");
                else
                {
                    throw new NotImplementedException("The automatic detection of child slots isn't yet implemented. If it is a problem, warn me :) (GameObject : " + name + ")");
                }
            }

            foreach (var slot in _slots)
            {
                slot.OnGrab.AddListener(Grab);
                slot.OnDrop.AddListener(Drop);
            }
        }

        public void Drop(IGrabbable item)
        {
            if (!_items.Contains(item)) return;

            _items.Remove(item);
            OnDrop?.Invoke(item);
        }

        public void Empty() { }

        public void Grab(IGrabbable item)
        {
            if (_items.Contains(item)) return;

            _items.Add(item);
            OnGrab?.Invoke(item);
        }

        /// <summary>
        /// Returns false, always. No item should be grabbed outside of the ones in the slots. And grabbing a slot isn't an option (yet?)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool CanGrab(IGrabbable item)
        {
            return false;
        }

        public void SetActive(bool active)
        {
            foreach(var slot in _slots)
            {
                slot.IsLocked = !active;
            }
        }

        public SlotContainer GetFirstAvailableSlot()
        {
            foreach(SlotContainer slot in _slots)
            {
                if (slot.IsFree && !slot.IsLocked) return slot;
            }

            return null;
        }

        public void Reset()
        {
            Player = null;
            _items = new();
            foreach (var slot in Slots)
                slot.HeldItem = null;

        }
        #endregion
    }

}