using Controller;
using GameSystem;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// The slot container can receive up to one IGrabbable object
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SlotContainer : MonoBehaviour, ISoloContainer, IWatchable
    {
        #region Properties
        public IGrabbable HeldItem { get => _heldItem; set { _heldItem = value; Debug.Log("HeldItem set"); } }

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
            _filter = gameObject.GetComponent<ContainerFilter>();
        }

        protected virtual void Update()
        {
            UpdateCollider();
            CheckIfLookedAt();
        }
        protected virtual void CheckIfLookedAt()
        {
            bool lookedAt = (Player != null && Player.LookTarget == this.gameObject);
            if (lookedAt && !_wasLookedAtThisFrame)
                _onLookAt?.Invoke();
            else if (!lookedAt && _wasLookedAtThisFrame)
                _onStopLookAt?.Invoke();
            _wasLookedAtThisFrame = lookedAt;
        }

        public virtual void Drop(IGrabbable item)
        {
            Debug.Log("The requested item to drop is " + item + ", and the possessed item is " + _heldItem + " (" + (item == _heldItem ? "EQUAL" : "NOT EQUAL") + ")");
            if (_heldItem == null || _heldItem != item) return;
            Empty();
            OnDrop?.Invoke(item);
            OnDrop_Raw.Invoke();
        }

        public virtual void Empty()
        {
           _heldItem = null;
        }

        public virtual void Grab(IGrabbable item)
        {
            if (!CanGrab(item))
                return;
            if (_itemSlot == null)
                throw new MissingReferenceException("Please reference the item slot's transform in this container slot's dedicated field : " + name);
            _heldItem = item;
            Debug.Log(this.name + " is grabbing item : " + (item as Item)?.name);

            ReferenceManager.Instance.GetElementOfType<Oven2>()?.SendMessage("Update"); 
            item.GetGrabbed(this);
            Debug.Log(this.name + " has grabbed item : " + (item as Item)?.name);

            ReferenceManager.Instance.GetElementOfType<Oven2>()?.SendMessage("Update");
            OnGrab?.Invoke(item);
        }

        public virtual bool CanGrab(IGrabbable item)
        {
            if (_heldItem != null) return false;
            if (_locked) return false;

            if (_filter == null)
                _filter = this.gameObject.GetComponent<ContainerFilter>();

            if (_filter != null && item is MonoBehaviour)
                if (!_filter.IsValid((MonoBehaviour)item))
                    return false;
            return true;
        }

        public virtual void GetGrabbed(IContainer newContainer)
        {
            if (newContainer == null) return;
            Container = newContainer;
            OnGrabbed?.Invoke();
        }

        public virtual void LookAt(PlayerInteractor interactor)
        {
            Player = interactor.Player;
        }

        protected virtual void UpdateCollider()
        {
            if (_collider == null) return;
            if (Player == null) return;
            if (_disableColliderIfHandEmpty)
                SetActive(Player.HeldItem != null && !_locked);
        }

        public virtual void SetAnimationMode(bool animate = true)
        {
        }
        #endregion

        protected enum ESlotFilter
        {
            All = 0,
            Ingredient = 1,
            MultiSlotContainer = 2
        }

        public virtual void SetActive(bool active)
        {
            if (_collider == null)
                _collider = GetComponent<Collider>();
            _collider.enabled = active;
        }

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
    }

}