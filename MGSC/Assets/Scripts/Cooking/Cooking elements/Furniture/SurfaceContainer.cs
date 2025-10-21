using Controller;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// The surface container is a surface that can receive any IGrabbable object on top of it.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SurfaceContainer : MonoBehaviour, IMultiContainer
    {
        #region Properties
        public PlayerInterface Player { get; set; }
        public UnityEvent<IGrabbable> OnGrab => _onGrab;
        public UnityEvent<IGrabbable> OnDrop => _onDrop;
        public List<IGrabbable> HeldItems => _items;
        public int MaxItemCount => _maxItemCount;
        #endregion

        #region Fields
        [Header("Surface container parameters")]
        [Tooltip("How many items can be put on this surface. Negative means infinite")]
        [SerializeField] private int _maxItemCount = 100;
        [Space]
        [Header("Surface container events")]
        [Tooltip("Invoked when an item is put onto the surface")]
        [SerializeField] private UnityEvent<IGrabbable> _onGrab;
        [Tooltip("Invoked when an item is taken out of the surface")]
        [SerializeField] private UnityEvent<IGrabbable> _onDrop;


        private List<IGrabbable> _items = new();
        private ContainerFilter _filter;

        #endregion

        #region Methods

        public void Grab(IGrabbable item)
        {
            if (_items.Contains(item)) return;

            _items.Add(item);
            item.GetGrabbed(this);
            OnGrab?.Invoke(item);
        }

        public void Drop(IGrabbable item)
        {
            if (!_items.Contains(item)) return;

            _items.Remove(item);
            OnDrop?.Invoke(item);
        }
        public bool CanGrab(IGrabbable item)
        {
            if (_filter == null)
                _filter = this.gameObject.GetComponent<ContainerFilter>();

            if (_filter != null && item is MonoBehaviour)
                if (!_filter.IsValid((MonoBehaviour)item))
                    return false;
            if (_maxItemCount < 0) return true;
            return _items.Count < _maxItemCount;
        }
        #endregion
    }

}