using Controller;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{

    public class TrashCan : Interactable, IContainer
    {
        #region Properties
        public UnityEvent<IGrabbable> OnGrab => _onGrab;
        public UnityEvent<IGrabbable> OnDrop => null; // The trashcan never drops anything for now
        public bool IsLocked => false;
        #endregion

        #region Fields
        [Header("Trashcan events")]
        [Tooltip("Invoked when an item is put into the trashcan")]
        [SerializeField] private UnityEvent<IGrabbable> _onGrab;
        #endregion

        public bool CanGrab(IGrabbable item)
        {
            if (item is not ITrashable) return false;
            //(item as ITrashable).Dispose();
            return true;
        }

        public void Drop(IGrabbable item) { }

        public void Grab(IGrabbable item)
        {
            if (!CanGrab(item)) return;
            item.GetGrabbed(this);
            OnGrab?.Invoke(item);
            Dispose(item);
        }

        public override void Interact(PlayerInteractor interactor)
        {
            base.Interact(interactor);

            Debug.Log("Interacting");
            if (interactor.HeldItem == null) return;
            if (!CanGrab(interactor.HeldItem)) return;
            Grab(interactor.HeldItem);
            interactor.Drop();
        }

        private void Dispose(IGrabbable item)
        {
            GameSystem.ReferenceManager.Instance.GetManager<ItemManager>().Despawn(item as Item);
        }
    }

}