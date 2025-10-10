using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Cooking;

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
        public PlayerInterface Interface { get => _playerInterface; set => _playerInterface = value; }
        public Action<PlayerInteractor> OnInteract { get; set; }
        public IGrabbable HeldItem { get=> _heldItem; set=> _heldItem=value; }
        public Transform ItemSlot => _handSlot;
        public Action<IGrabbable> OnGrab { get; set; }
        public Action<IGrabbable> OnDrop { get; set; }

        #endregion

        #region Fields
        [Header("References")]
        [Tooltip("Transform for the slot where objects are put when grabbed")]
        [SerializeField] private Transform _handSlot;
        [Tooltip("If the player should hold on to an item on start, reference it here")]
        [SerializeField] private BaseObject _defaultHeldItem;
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
                BaseObject target;
                if (LookTargetIsValid(out target))
                {
                    target.LookAt(this);
                }
                IContainer container;
                if (LookTargetIsContainer(out container))
                {
                    LookContainer = container;
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
            BaseObject target;
            if (!LookTargetIsValid(out target)) return;

            target.Interact(this);
            OnInteract?.Invoke(this);
        }

        /// <summary>
        /// Check if the looked at target has a component derived from BaseObject, and if so, passes it back as out parameter. Otherwise it will be null.
        /// </summary>
        /// <param name="interactable">The interactable that the player is looking at</param>
        /// <returns></returns>
        private bool LookTargetIsValid(out BaseObject baseObject)
        {
            if (LookTarget != null)
            {
                foreach (var comp in LookTarget.GetComponents<Component>())
                {
                    if (comp is BaseObject)
                    {
                        baseObject = comp as BaseObject;
                        return true;
                    }
                }
            }
            baseObject = null;
            return false;
        }
        /// <summary>
        /// Check if the looked at target has a component implementing IContainer, and if so, passes it back as out parameter. Otherwise it will be null.
        /// </summary>
        /// <param name="interactable">The interactable that the player is looking at</param>
        /// <returns></returns>
        private bool LookTargetIsContainer(out IContainer container)
        {
            if (LookTarget != null)
            {
                foreach (var comp in LookTarget.GetComponents<Component>())
                {
                    if (comp is IContainer)
                    {
                        container = (IContainer)comp;
                        return true;
                    }
                }
            }
            container = null;
            return false;
        }



        public void GrabOrDrop(InputAction.CallbackContext context)
        {
            if (HeldItem == null) Grab();
            else Drop();
        }

        public void Grab()
        {
            BaseObject target;
            if (!LookTargetIsValid(out target)) return;
            Grab(target);
        }
        public void Grab(IGrabbable item)
        {
            if (item == null) return;
            if (_heldItem != null) return;

            _grabbedItemThisFrame = true;
            item.GetGrabbed(this);
            _heldItem = item;
            OnGrab?.Invoke(item);
        }

        public void Drop(IGrabbable item) => Drop();
        public void Drop()
        {
            bool checkOverlap = true;
            if (_heldItem == null) return;
            if (_grabbedItemThisFrame) return;

            IContainer container;
            if (!LookTargetIsContainer(out container)) return;

            if (checkOverlap && container is IMultiContainer && _heldItem is BaseObject)
            {
                bool isValid = BaseObject.CheckValidSpot((BaseObject)_heldItem, LookInfo.point);
                if (!isValid) return;
            }

            container.Grab(_heldItem);
            OnDrop?.Invoke(_heldItem);
            _heldItem = null;
        }


        #endregion
    }

}