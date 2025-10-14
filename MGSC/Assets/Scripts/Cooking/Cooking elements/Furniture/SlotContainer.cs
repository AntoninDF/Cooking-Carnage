using Controller;
using System;
using UnityEngine;
using UnityEngine.Events;

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

        public UnityEvent<IGrabbable> OnGrab => _onGrab;
        public UnityEvent<IGrabbable> OnDrop => _onDrop;
        public PlayerInterface Player { get; set; }
        public bool IsEmpty => _heldItem == null;

        public IContainer Container { get; set; }
        public UnityEvent OnGrabbed { get; set; }

        public Collider Collider => _collider;

        public bool IsLookedAt => Player.LookTarget == gameObject;
        #endregion

        #region Fields
        [Header("Slot references")]
        [Tooltip("Transform designating where the contained item should go")]
        [SerializeField] private Transform _itemSlot;
        [Tooltip("Filter which type of object can be contained in this slot")]
        [SerializeField] private ESlotFilter _filter;
        [Space]
        [Header("Slot events")]
        [Tooltip("Invoked when an item is put into the slot")]
        [SerializeField] private UnityEvent<IGrabbable> _onGrab;
        [Tooltip("Invoked when an item is taken out of the slot")]
        [SerializeField] private UnityEvent<IGrabbable> _onDrop;


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
            if (!CanGrab(item))
                throw new Exception("The grab method was called, but the target container " +
                    "cannot receive the given item. Please check it with CanGrab before calling Grab.");
            if (_itemSlot == null)
                throw new MissingReferenceException("Please reference the item slot's transform in this container slot's dedicated field : " + name);

            _heldItem = item;
            item.GetGrabbed(this);
            OnGrab?.Invoke(item);
        }

        public bool CanGrab(IGrabbable item)
        {
            if (_heldItem != null) return false;
            switch (_filter)
            {
                default:
                case ESlotFilter.All:
                    return true;
                case ESlotFilter.Ingredient:
                    if (item is not Ingredient) return false;
                    break;
                case ESlotFilter.MultiSlotContainer:
                    if (item is not MultiSlotContainer && 
                        !item.transform.gameObject.HasComponent<MultiSlotContainer>()) 
                        return false;
                    break;
            }
            return true;
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

        private enum ESlotFilter
        {
            All = 0,
            Ingredient = 1,
            MultiSlotContainer = 2
        }
    }

}