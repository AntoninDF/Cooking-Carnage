using Controller;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    [RequireComponent(typeof(Collider))]
    public class Oven : MonoBehaviour, IGrabbable
    {
        #region Properties
        public UnityEvent OnStartCooking => _onStartCooking;
        public UnityEvent OnStopCooking => _onStopCooking;
        public UnityEvent OnFailedStartAttempt => _onFailedStartAttempt;
        public UnityEvent OnItemCooked => _onItemCooked;
        public UnityEvent OnItemOvercooked => _onItemOvercooked;
        public bool IsCooking => _isCooking;
        public float Strength => _strength;

        public IContainer Container { get; set; }

        public UnityEvent OnGrabbed => _onGrabbed;

        public Collider Collider => _collider;

        public PlayerInterface Player { get; set; }

        #endregion

        #region Fields
        [Header("Oven references")]
        [Tooltip("The slot on which the plate will be put")]
        [SerializeField] private SlotContainer _slot;
        [Tooltip("The furnace needs a reference to it's own door to ensure it's closed while cooking :)")]
        [SerializeField] private Door _door;
        [Space]
        [Header("Oven parameters")]
        [Tooltip("Speed modifier for the oven")]
        [SerializeField] private float _strength = 1.0f;
        [Tooltip("Does the door's state impact whether the oven can cook or not")]
        [SerializeField] private bool _requireClosedDoor = true;
        [Space]
        [Header("Oven events")]
        [Tooltip("Invoked when the furnace starts cooking")]
        [SerializeField] private UnityEvent _onStartCooking;
        [Tooltip("Invoked when the furnace stops cooking")]
        [SerializeField] private UnityEvent _onStopCooking;
        [Tooltip("Invoked when the player tries to start the furnace, but it cannot be started (if the door is opened for example)")]
        [SerializeField] private UnityEvent _onFailedStartAttempt;
        [Tooltip("Invoked when the item is cooked")]
        [SerializeField] private UnityEvent _onItemCooked;
        [Tooltip("Invoked when the item is overcooked")]
        [SerializeField] private UnityEvent _onItemOvercooked;

        private bool _isCooking = false;
        private ECookStatus _currentStatus = ECookStatus.Raw;

        private UnityEvent _onGrabbed;
        private Collider _collider;
        #endregion

        #region Methods

        private void Start()
        {
            _collider = GetComponent<Collider>();
            if (_door == null)
                throw new MissingReferenceException("Please assign a reference to the furnace's door in the inspector :)");
        }

        private void Update()
        {
            TryCook();
        }

        public void ToggleCooking()
        {
            if (_isCooking)
            {
                StopCooking();
                return;
            }
            if ((_requireClosedDoor && _door.IsOpen) || _slot.IsFree)
            {
                OnFailedStartAttempt?.Invoke();
                return;
            }
            StartCooking();
        }

        private void TryCook()
        {
            if (!_isCooking) return;

            if ((_requireClosedDoor && _door.IsOpen) || _slot.IsFree)
            {
                StopCooking();
                return;
            }
            Cook();
        }

        private void Cook()
        {
            ICookable cookItem = _slot.HeldItem as ICookable;
            if (cookItem == null) return;
            cookItem.Cook(Time.deltaTime * _strength);
            if (_currentStatus != cookItem.CookStatus)
            {
                _currentStatus = cookItem.CookStatus;
                if (_currentStatus == ECookStatus.Cooked)
                {
                    _onItemCooked?.Invoke();
                }
                if (_currentStatus == ECookStatus.Overcooked)
                {
                    _onItemOvercooked?.Invoke();
                }
            }
        }

        public void StartCooking()
        {
            _isCooking = true;
            OnStartCooking?.Invoke();
        }

        private void StopCooking()
        {
            _isCooking = false;
            _currentStatus = ECookStatus.Raw;
            ICookable cookItem = _slot.HeldItem as ICookable;
            OnStopCooking?.Invoke();
            if (cookItem == null) return;
            cookItem.StopCooking();
        }

        public void StopCooking(IGrabbable item)
        {
            StopCooking();
            if (item is ICookable)
                (item as ICookable).StopCooking();
        }

        public void GetGrabbed(IContainer newContainer)
        {
            if (newContainer is not SlotContainer) return;
            Container?.Drop(this);
            Container = newContainer;
            if (newContainer is ISoloContainer)
            {
                ISoloContainer soloContainer = (ISoloContainer)newContainer;
                transform.SetParent(soloContainer.ItemSlot);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                transform.parent = newContainer.transform;
            }
            OnGrabbed?.Invoke();
        }

        public void SetAnimationMode(bool animate = true) { }
        public void SetStrength(float strength) { _strength = strength; }

        #endregion
    }
}