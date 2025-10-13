using Controller;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{

    public class TrashCan : Interactable, IContainer
    {
        #region Properties
        public UnityEvent<IGrabbable> OnGrab => _onGrab;
        public UnityEvent<IGrabbable> OnDrop => null; // The trashcan never drops anything for now
        #endregion

        #region Fields
        [Header("Trashcan events")]
        [Tooltip("Invoked when an item is put into the trashcan")]
        [SerializeField] private UnityEvent<IGrabbable> _onGrab;
        #endregion

        public bool CanGrab(IGrabbable item)
        {
            if (item is not Ingredient) return false;
            return true;
        }

        public void Drop(IGrabbable item) { }

        public void Grab(IGrabbable item)
        {
            OnGrab?.Invoke(item);
            Dispose(item);
        }

        public override void Interact(PlayerInteractor interactor)
        {
            base.Interact(interactor);

            if (interactor.HeldItem == null) return;

            interactor.Drop();
        }

        private void Dispose(IGrabbable item)
        {
            Destroy(item.transform.gameObject);
        }
    }

}