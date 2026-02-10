using Cooking;
using GameSystem;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Controller
{

    public class PlayerInteractor : MonoBehaviour, ISoloContainer, ICallbackBroadcaster
    {
        #region Properties
        public PlayerInputHandler InputHandler => Player.InputHandler;
        public GameObject LookTarget { get; private set; }
        public RaycastHit LookInfo { get; private set; }
        public IContainer LookContainer { get; private set; }
        public float ReachDistance { get => _reachDistance; set => _reachDistance = value; }
        [field: SerializeField]
        public UnityEvent<PlayerInteractor> OnInteract { get; set; }
        public IGrabbable HeldItem { get => _heldItem; set => _heldItem = value; }
        public Transform ItemSlot => _handSlot;
        [field: SerializeField]
        public UnityEvent<IGrabbable> OnGrab { get; set; }
        [field: SerializeField]
        public UnityEvent<IGrabbable> OnDrop { get; set; }
        public PlayerInterface Player { get => _playerInterface; set => _playerInterface = value; }
        public bool IsLocked { get; set; }
        public bool EnableCallbacks { get; set; }
        public bool Passthrough => false;

        public ICallbackBroadcaster Self => this;

        #endregion


        #region Fields
        [Header("References")]
        [Tooltip("Transform for the slot where objects are put when grabbed")]
        [SerializeField] private Transform _handSlot;
        [Tooltip("If the player should hold on to an item on start, reference it here")]
        [SerializeField] private IGrabbable _defaultHeldItem;
        [Space()]
        [Header("Parameters")]
        [Tooltip("Distance in meters up to which the player can reach items")]
        [SerializeField] private float _reachDistance = 3f;

        [BindEvent("Player/OnInteract")]
        public Action _onInteract;

        private PlayerInterface _playerInterface;
        private IGrabbable _heldItem;
        private bool _grabbedItemThisFrame;
        private RaycastHit[] _lookedAtObjects;
        private TimerManager _timerManager;

        #endregion

        #region Methods

        void Start()
        {
            if (InputHandler == null)
                throw new MissingReferenceException("The player input handler wasn't assigned by the GameInitializer.");

            InputHandler.OnInteract += TryPlayerAction;

            if (_handSlot == null)
                throw new MissingReferenceException("Please assign the hand slot reference in the player interactor :)");

            TryGrab(_defaultHeldItem);

            Reference.GetManager(out _timerManager);
        }

        public Transform GetLookTransform() => Camera.main.transform;
        public RaycastHit[] GetLookedAtObjects() => _lookedAtObjects;

        /// <summary>
        /// Update the LookInfo and LookTarget variables to inform on what is being looked at.
        /// </summary>
        private void UpdateLookInfo()
        {
            LookContainer = null;
            RaycastHit[] allHits;
            RaycastHit hit;
            allHits = Physics.RaycastAll(GetLookTransform().position, GetLookTransform().rotation * Vector3.forward);
            if (allHits.Length > 0 && allHits[0].distance <= _reachDistance)
            {
                hit = allHits[0];
                LookInfo = hit;
                LookTarget = LookInfo.collider.gameObject;
                Component target;
                if (LookTargetImplements<IWatchable>(out target))
                {
                    ((IWatchable)target).LookAt(this);
                }
                Component container;
                if (LookTargetImplements<IContainer>(out container))
                {
                    LookContainer = container as IContainer;
                }
            }
            else
            {
                LookTarget = null;
                LookInfo = default(RaycastHit);
            }
            _lookedAtObjects = allHits;
        }
        /// <summary>
        /// Check if the looked at target has a component derived from BaseObject, and if so, passes it back as out parameter. Otherwise it will be null.
        /// </summary>
        /// <param name="interactable">The interactable that the player is looking at</param>
        /// <returns></returns>
        private bool LookTargetImplements<T>(out Component component) where T : IGeneric<T>
        {
            if (LookTarget != null)
            {
                foreach (var comp in LookTarget.GetComponents<Component>())
                {
                    if (comp is T)
                    {
                        component = comp;
                        return true;
                    }
                }
            }
            component = null;
            return false;
        }

        #region IContainer methods
        public bool RequestGrab(IGrabbable item)
        {
            // Try to grab the item
            if (CanGrab(item))
                return TryGrab(item);
            // Or try to ask the held item to grab the item
            if (HeldItem is IContainer container)
                return container.RequestGrab(item);
            return false;
        }
        public bool RequestDrop(IGrabbable item = null)
        {
            return TryDrop(item);
        }
        protected bool TryGrab(IGrabbable item)
        {
            // Check if this container can grab the item
            if (!CanGrab(item))
                return false;
            // Check if the item's container is ok to drop it's item
            if (item.Container != null)
            {
                if (!item.Container.CanDrop(item)) return false;
            }

            Grab(item);
            return true;
        }
        protected bool TryDrop(IGrabbable item = null)
        {
            if (!CanDrop(item))
                return false;
            Drop(item);
            return false;
        }
        protected void Grab(IGrabbable item)
        {
            Self.Callback(OnGrab, item);
            _heldItem = item;
            item.Container?.RequestDrop(item);
            item.GetGrabbedBy(this);
        }
        protected void Drop(IGrabbable item = null)
        {
            if (_heldItem == null) return;
            _timerManager.StartTimer("Enable dropped item's collider", 0.1f, () =>
            {
                _heldItem.Collider.enabled = true;
                _heldItem = null;
            }
            );
            Self.Callback(OnDrop, item);
        }
        public void Empty()
        {
            _heldItem = null;
        }
        public bool CanGrab(IGrabbable item)
        {
            if (item == null) return false;
            if (item.IsLocked) return false;
            if (item.Container.IsLocked) return false;
            if (IsLocked) return false;
            return true;
        }
        public bool CanDrop(IGrabbable item)
        {
            if (IsLocked) return false;
            if (item == null) return false;
            if (item.IsLocked) return false;
            return true;
        }
        #endregion

        // PRIVATE METHODS
        private void TryPlayerAction(InputAction.CallbackContext ctx) => TryPlayerAction();
        private bool TryPlayerAction()
        {
            Component comp;
            if (LookTargetImplements<IInteractable>(out comp))
            {
                (comp as IInteractable).Interact(this);
                Self.Callback(OnInteract, this);
            }
            if (LookTargetImplements<IGrabbable>(out comp))
            {
                RequestGrab(comp as IGrabbable);
            }
            if (LookTargetImplements<IContainer>(out comp))
            {
                var container = comp as IContainer;
                if (container.Passthrough)
                {
                    RequestGrab(container.HeldItem);
                }
                else
                {
                    container.RequestGrab(_heldItem);
                }
            }
            return false;
        }

        #endregion
    }
}