using Controller;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// The surface container is a surface that can receive any IGrabbable object on top of it.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SurfaceContainer : MonoBehaviour, IMultiContainer, ICallbackBroadcaster
    {
        #region Properties
        public PlayerInterface Player { get; set; }
        public UnityEvent<IGrabbable> OnGrab => _onGrab;
        public UnityEvent<IGrabbable> OnDrop => _onDrop;
        public List<IGrabbable> HeldItems => _items;
        public IGrabbable HeldItem => _items == null ? null : _items.Count > 0 ? _items[0] : null;
        public int MaxItemCount => _maxItemCount;
        public bool IsLocked { get; set; }
        public bool Passthrough => false;
        public bool EnableCallbacks { get; set; }
        public ICallbackBroadcaster Self => this;
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

        public bool RequestGrab(IGrabbable item)
        {
            return TryGrab(item);
        }
        public bool RequestDrop(IGrabbable item)
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
            if (!CanDrop(item))
                return false;
            Drop(item);
            return true;
        }
        

        protected void Grab(IGrabbable item)
        {
            _items.Add(item);
            item.GetGrabbedBy(this);
            Self.Callback(OnGrab, item);
        }
        protected void Drop(IGrabbable item)
        {
            _items.Remove(item);
            Self.Callback(OnDrop, item);
        }
        public bool CanGrab(IGrabbable item)
        {
            if (item == null) return false;
            if (IsLocked) return false;
            if (item.IsLocked) return false;
            if (item.Container != null && !item.Container.CanDrop(item)) return false;

            if ((_filter != null || TryGetComponent(out _filter)) && item is MonoBehaviour)
                if (!_filter.IsValid((MonoBehaviour)item))
                    return false;

            if (_maxItemCount < 0) return true;
            if (_items.Count >= _maxItemCount) return false;
            if (_items.Contains(item)) return false;

            return true;
        }
        public bool CanDrop(IGrabbable item)
        {
            if (item == null) return false;
            if (IsLocked) return false;
            if (item.IsLocked) return false;
            if (!_items.Contains(item) ) return false;

            return true;
        }
        public void Empty()
        {
            for (int i = _items.Count-1; i >= 0; i--)
            {
                Drop(_items[i]);
            }
        }
        #endregion
    }

}