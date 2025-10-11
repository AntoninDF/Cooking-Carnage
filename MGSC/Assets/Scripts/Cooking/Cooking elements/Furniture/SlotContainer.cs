using Controller;
using System;
using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// The slot container can receive up to one IGrabbable object
    /// </summary>
    public class SlotContainer : MonoBehaviour, ISoloContainer, IGrabbable, IWatchable
    {
        #region Properties
        public IGrabbable HeldItem => _heldItem;

        public Transform ItemSlot => _itemSlot;

        public Action<IGrabbable> OnGrab { get; set; }
        public Action<IGrabbable> OnDrop { get; set; }
        public PlayerInterface Player { get; set; }
        public bool IsEmpty => _heldItem == null;

        public IContainer Container { get; set; }
        public Action OnGrabbed { get; set; }

        public Collider Collider => _collider;

        public bool IsLookedAt => Player.LookTarget == gameObject;
        #endregion

        #region Fields
        [Header("References")]
        [Tooltip("Transform designating where the contained item should go")]
        [SerializeField] private Transform _itemSlot;

        private Collider _collider;
        private IGrabbable _heldItem;
        #endregion

        #region Methods

        void Start()
        {
            _collider = GetComponent<Collider>();
        }

        private void Update()
        {
            UpdateCollider();
        }

        public void Drop(IGrabbable item)
        {
            if (_heldItem == null) return;
            _heldItem = null;
            OnDrop?.Invoke(item);
        }

        public void Grab(IGrabbable item)
        {
            if (_heldItem != null) return;
            if (_itemSlot == null)
                throw new MissingReferenceException("Please reference the item slot's transform in this container slot's dedicated field : " + name);
            _heldItem = item;
            item.GetGrabbed(this);
            OnGrab?.Invoke(item);
        }

        public void GetGrabbed(IContainer newContainer)
        {
            if (newContainer == null) return;
            Container = newContainer;
            OnGrabbed?.Invoke();
        }

        public void LookAt(PlayerInteractor interactor)
        {
            Player = interactor.Player;
        }

        private void UpdateCollider()
        {
            if (_collider == null) return;
            if (Player == null) return;

            _collider.enabled = Player.HeldItem != null;
        }
        #endregion
    }

}