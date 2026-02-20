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
    public class MultiSlotContainer : MonoBehaviour, IFeatureCollection
    {
        #region Properties
        
        public List<SlotContainer> Slots => _slots;
        public bool IsLocked { get; set; }
        public bool Passthrough => false;

        public bool EnableCallbacks { get; set; }

        public FeatureCollection Collection { get; set; } = new();

        #endregion

        #region Fields
        [Header("Multislot references")]
        [Tooltip("List of all the slots that belong to this multi-slot container")]
        [SerializeField] private List<SlotContainer> _slots = new ();
        [Tooltip("Wether or not this container should scan it's hierarchy on Start to find it's own slots")]
        [SerializeField] private bool _detectSlots;
        [Space]


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
                //slot.OnGrab.AddListener(RecordGrab);
                //slot.OnDrop.AddListener(RecordDrop);
            }
        }

        #endregion
    }

}