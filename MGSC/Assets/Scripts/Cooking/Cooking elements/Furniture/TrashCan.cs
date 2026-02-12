using Controller;
using GameSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{

    public class TrashCan : MonoBehaviour, IContainer, ICallbackBroadcaster
    {
        #region Properties
        public PlayerInterface Player {  get; set; }
        public UnityEvent<IGrabbable> OnGrab => _onGrab;
        public UnityEvent<IGrabbable> OnDrop => null; // The trashcan never drops anything for now
        public bool IsLocked { get; set; }
        public bool Passthrough => false;
        public IGrabbable HeldItem => null; // No item can ever be retrieved from the trashcan

        public bool EnableCallbacks { get; set; }

        public ICallbackBroadcaster Self => this;

        #endregion

        #region Fields
        [Header("Trashcan events")]
        [Tooltip("Invoked when an item is put into the trashcan")]
        [SerializeField] private UnityEvent<IGrabbable> _onGrab;

        private ItemManager _itemManager;

        #endregion

        void Start()
        {
            Reference.GetManager(out _itemManager);
        }

        public bool RequestGrab(IGrabbable item) => TryGrab(item);
        public bool RequestDrop(IGrabbable item) => TryDrop(item);

        public bool CanGrab(IGrabbable item)
        {
            if (item is not ITrashable) return false;
            return true;
        }
        public bool CanDrop(IGrabbable item) => false; // No item can ever come out of the trashcan
        public void Empty() { }
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
            item.GetGrabbedBy(this);
            Self.Callback(OnGrab, item);
            Dispose(item);
        }
        protected void Drop(IGrabbable item) { }

        protected void Dispose(IGrabbable item)
        {
            _itemManager.Despawn(item as Item);
        }
    }

}