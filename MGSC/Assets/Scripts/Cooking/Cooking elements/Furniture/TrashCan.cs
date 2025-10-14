using Controller;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace Cooking
{

    public class TrashCan : Interactable, IContainer
    {
        public Action<IGrabbable> OnGrab {get;set;}
        public Action<IGrabbable> OnDrop { get; set; }

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