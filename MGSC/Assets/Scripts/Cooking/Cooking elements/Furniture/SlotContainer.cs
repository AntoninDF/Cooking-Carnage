using Controller;
using GameSystem;
using System;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// The slot container can receive up to one IGrabbable object
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SlotContainer : MonoBehaviour, ISoloContainer, IWatchable, ICallbackBroadcaster
    {
        #region Properties
        public IGrabbable HeldItem { get => _heldItem; set { _heldItem = value; } }

        public Transform ItemSlot => _itemSlot;

        public UnityEvent<IGrabbable> OnGrab => _onGrab;
        public UnityEvent<IGrabbable> OnDrop => _onDrop;
        public UnityEvent OnDrop_Raw;
        public PlayerInterface Player { get; set; }
        public bool IsFree => _heldItem == null && !_locked;
        public bool IsLocked { get => _locked; set => _locked = value; }

        public IContainer Container { get; set; }
        public UnityEvent OnGrabbed { get; set; }
        public UnityEvent OnLookAt => _onLookAt;
        public UnityEvent OnStopLookAt => _onStopLookAt;

        public Collider Collider => _collider;
        public ContainerFilter Filter => _filter;

        public bool IsLookedAt => Player.LookTarget == gameObject;

        public bool Passthrough => false;

        public bool EnableCallbacks { get; set ; }

        public ICallbackBroadcaster Self => this;
        #endregion

        #region Fields
        [Header("Parameters")]
        [SerializeField]
        protected bool _disableColliderIfHandEmpty = true;
        [Header("Slot references")]
        [Tooltip("Transform designating where the contained item should go")]
        [SerializeField] protected Transform _itemSlot;
        [Space]
        [Header("Slot events")]
        [Tooltip("Invoked when an item is put into the slot")]
        [SerializeField] protected UnityEvent<IGrabbable> _onGrab;
        [Tooltip("Invoked when an item is taken out of the slot")]
        [SerializeField] protected UnityEvent<IGrabbable> _onDrop;
        [SerializeField] protected UnityEvent _onLookAt;
        [SerializeField] protected UnityEvent _onStopLookAt;
        [SerializeField] protected UnityEvent _onTryGrabIncorrectItem;


        protected Collider _collider;
        protected IGrabbable _heldItem;
        protected ContainerFilter _filter;
        protected bool _locked;
        protected bool _wasLookedAtThisFrame;
        #endregion

        #region Methods

        void Start()
        {
            _collider = GetComponent<Collider>();
            _filter = GetComponent<ContainerFilter>();
        }

        protected virtual void Update()
        {
            UpdateCollider();
            CheckIfLookedAt();
        }
        protected virtual void UpdateCollider()
        {
            if (_collider == null) return;
            if (Player == null) return;
            if (_disableColliderIfHandEmpty)
                SetActive(Player.HeldItem != null && !_locked);
        }
        public virtual void SetActive(bool active)
        {
            if (_collider == null)
                _collider = GetComponent<Collider>();
            _collider.enabled = active;
        }
        protected virtual void CheckIfLookedAt()
        {
            bool lookedAt = (Player != null && Player.LookTarget == this.gameObject);
            if (lookedAt && !_wasLookedAtThisFrame)
                Self.Callback(_onLookAt);
            else if (!lookedAt && _wasLookedAtThisFrame)
                Self.Callback(_onStopLookAt);
            _wasLookedAtThisFrame = lookedAt;
        }
        public virtual void Empty() =>_heldItem = null;
        /*
        public virtual void GetGrabbedBy(IContainer newContainer)
        {
            if (newContainer == null) return;
            Container = newContainer;
            OnGrabbed?.Invoke();
        }*/
        public virtual void LookAt(PlayerInteractor interactor) => Player = interactor.Player;
        
        public virtual void SetAnimationMode(bool animate = true) { }
        #endregion


        public virtual void SetFilter(params Filter[] filters)
        {
            if (_filter != null)
            {
                _filter.Clear();
            }
            else
            {
                _filter = gameObject.AddComponent<ContainerFilter>();
            }
            foreach (var filter in filters)
                _filter.AddFilter(filter);
        }

        public bool RequestGrab(IGrabbable item)
        {
            return TryGrab(item);
        }

        public bool RequestDrop(IGrabbable item = null)
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
        protected bool TryDrop(IGrabbable item = null)
        {
            if (!CanDrop(item))
                return false;
            Drop(item);
            return true;
        }
        protected virtual void Grab(IGrabbable item)
        {
            _heldItem = item;
            item.GetGrabbedBy(this);
            Self.Callback(OnGrab, item);
        }
        protected virtual void Drop(IGrabbable item = null)
        {
            _heldItem = null;
            Self.Callback(OnDrop, item);
            Self.Callback(OnDrop_Raw);
        }
        public virtual bool CanGrab(IGrabbable item)
        {
            if (_heldItem != null) return false;
            if (_locked) return false;
            if (_itemSlot == null)

                if ((_filter != null || TryGetComponent(out _filter)) && item is MonoBehaviour)
                    if (!_filter.IsValid((MonoBehaviour)item))
                    {
                        if (!(item is SlotContainer))
                        {
                            Self.Callback(_onTryGrabIncorrectItem);
                        }

                        return false;
                    }
            return true;
        }
        public bool CanDrop(IGrabbable item = null)
        {
            if (IsLocked) return false;
            if (HeldItem == null) return false;
            if (HeldItem.IsLocked) return false;
            // if (item != HeldItem) return false; // Uncomment this to make sure the right item to be dropped is requested. Otherwise it will drop whatever the requested item is
            return true;
        }
    }

}