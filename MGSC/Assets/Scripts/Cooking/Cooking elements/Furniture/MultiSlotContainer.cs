using Controller;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// The multi slot container contains slots, that hold items themselves. The multi slot container is useful to keep track of the content of a fixed set of items.
    /// </summary>
    public class MultiSlotContainer : MonoBehaviour, IMultiContainer
    {
        #region Properties
        public List<IGrabbable> HeldItems => _items;
        public Action<IGrabbable> OnGrab { get; set; }
        public Action<IGrabbable> OnDrop { get; set; }
        public PlayerInterface Player { get; set; }
        public List<SlotContainer> Slots => _slots;

        #endregion

        #region Fields
        [Header("Multislot references")]
        [Tooltip("List of all the slots that belong to this multi-slot container")]
        [SerializeField] private List<SlotContainer> _slots = new ();
        [Tooltip("Wether or not this container should scan it's hierarchy on Start to find it's own slots")]
        [SerializeField] private bool _detectSlots;
        List<IGrabbable> _items = new();

        #endregion

        #region Methods

        private void Start()
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
                slot.OnGrab += Grab;
                slot.OnDrop += Drop;
            }
                
        }


        public void Drop(IGrabbable item)
        {
            if (!_items.Contains(item)) return;

            _items.Remove(item);
            OnDrop?.Invoke(item);
        }

        public void Grab(IGrabbable item)
        {
            if (_items.Contains(item)) return;

            _items.Add(item);
            OnGrab?.Invoke(item);
        }
        #endregion
    }

}