using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// The container surface is a surface that can receive any IGrabbable object on top of it.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ContainerSurface : MonoBehaviour, IMultiContainer
    {
        #region Properties
        public List<IGrabbable> Items => _items;

        public Action<IGrabbable> OnGrab { get ; set; }
        public Action<IGrabbable> OnDrop { get; set; }
        public List<IGrabbable> HeldItems { get => _items; set => _items = value; }
        #endregion

        #region Fields
        private List<IGrabbable> _items = new();

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
        #endregion
    }

}