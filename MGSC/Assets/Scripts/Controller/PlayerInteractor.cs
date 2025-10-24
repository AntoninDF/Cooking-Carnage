using Cooking;
using Mono.Cecil;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Controller
{

    public class PlayerInteractor : MonoBehaviour, ISoloContainer
    {
        #region Properties
        public PlayerInputHandler InputHandler { get => _playerInputs; set => _playerInputs = value; }
        public GameObject LookTarget { get; private set; }
        public RaycastHit LookInfo { get; private set; }
        public IContainer LookContainer { get; private set; }
        public float ReachDistance { get => _reachDistance; set => _reachDistance = value; }
        [field: SerializeField]
        public UnityEvent<PlayerInteractor> OnInteract { get; set; }
        public IGrabbable HeldItem { get=> _heldItem; set=> _heldItem=value; }
        public Transform ItemSlot => _handSlot;
        [field: SerializeField]
        public UnityEvent<IGrabbable> OnGrab { get; set; }
        [field: SerializeField]
        public UnityEvent<IGrabbable> OnDrop { get; set; }
        public PlayerInterface Player { get => _playerInterface; set => _playerInterface = value; }
        public bool IsLocked => false;

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

        private PlayerInterface _playerInterface;
        private PlayerInputHandler _playerInputs;
        private IGrabbable _heldItem;
        private bool _grabbedItemThisFrame;
        #endregion


        #region Methods
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (_playerInputs == null)
                throw new MissingReferenceException("The player input handler wasn't assigned by the GameInitializer.");

            _playerInputs.OnInteract += Interact;
            _playerInputs.OnGrab += GrabOrDrop;

            if (_handSlot == null)
                throw new MissingReferenceException("Please assign the hand slot reference in the player interactor :)");

            Grab(_defaultHeldItem);
        }

        // Update is called once per frame
        void Update()
        {
            UpdateLookInfo();
            _grabbedItemThisFrame = false;
        }

        public Transform GetLookTransform() => Camera.main.transform;

        /// <summary>
        /// Update the LookInfo and LookTarget variables to inform on what is being looked at.
        /// </summary>
        private void UpdateLookInfo()
        {
            LookContainer = null;
            RaycastHit hit;
            if (Physics.Raycast(GetLookTransform().position, GetLookTransform().rotation * Vector3.forward, out hit, _reachDistance))
            {
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
        }

        private void Interact(InputAction.CallbackContext context)
        {
            Component target;
            if (!LookTargetImplements<IInteractable>(out target)) return;

            ((IInteractable)target).Interact(this);
            OnInteract?.Invoke(this);
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



        public void GrabOrDrop(InputAction.CallbackContext context)
        {
            if (HeldItem == null) Grab();
            else DropOnContainer();
        }

        public void Grab()
        {
            Component target;
            if (!LookTargetImplements<IGrabbable>(out target)) return;
            IGrabbable t = target as IGrabbable;
            t.SetAnimationMode(true);
            Grab(t);
        }
        public void Grab(IGrabbable item)
        {
            if (item == null) return;
            if (_heldItem != null) return;
            if (item.Container != null && item.Container.IsLocked) return;

            _grabbedItemThisFrame = true;
            item.GetGrabbed(this);
            _heldItem = item;
            _heldItem.Collider.enabled = false;
            OnGrab?.Invoke(item);
        }

        public void Drop(IGrabbable item) => Drop();
        public void DropOnContainer()
        {
            bool checkOverlap = true;
            if (_heldItem == null) return;
            if (_grabbedItemThisFrame) return;

            Component component;
            if (!LookTargetImplements<IContainer>(out component)) return;

            IContainer container = component as IContainer;
            if (!container.CanGrab(_heldItem)) return;

            if (checkOverlap && component is IMultiContainer)
            {
                // REFACTO - TO DO : Implement overlap checking. Return if there is an overlap, otherwise continue
            }

            //Finally grab
            container.Grab(_heldItem);
        }

        public void Drop()
        {
            _heldItem.Collider.enabled = true;
            OnDrop?.Invoke(_heldItem);
            _heldItem = null;
        }

        public bool CanGrab(IGrabbable item) => _heldItem == null;

        #endregion
    }

}