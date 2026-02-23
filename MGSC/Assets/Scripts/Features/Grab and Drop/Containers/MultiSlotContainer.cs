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
        public MultiContainer Container { get; set; }
        #endregion

        #region Fields
        [Header("Multislot references")]
        [Tooltip("List of all the slots that belong to this multi-slot container")]
        [SerializeField] private List<SlotContainer> _slots = new ();
        [Tooltip("Wether or not this container should scan it's hierarchy on Start to find it's own slots")]
        [SerializeField] private bool _detectSlots;
        [Space]


        private ContainerFilter _filter;
        private List<SoloContainer> _slotComponents;
        #endregion

        #region Methods

        private void Start()
        {
            Container = new(this);
            Filter = new (this);
            Init();
        }

        private void Init()
        {
            if (_slots.Count == 0 && !_detectSlots)
            {
                Debug.LogWarning("Please reference the slots for this multi-slot container in the inspector, or opt for automatic detection. (GameObject : " + name + ")");
            }

            if (_detectSlots) _slots = GetComponentsInChildren<SlotContainer>().ToList();
            _slotComponents = new FeatureScannerByHierarchy(gameObject).Get<SoloContainer>();


            Container.Filter.AddCondition
            Container.OnGrab += GrabByFirstAvailableSlot;
        }

        private void GrabByFirstAvailableSlot(Grabbable grabbable)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}