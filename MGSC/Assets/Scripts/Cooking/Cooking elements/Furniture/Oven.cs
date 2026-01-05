using Controller;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using GameSystem;
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
        public UnityEvent OnItemDropped => _slot.OnDrop_Raw;
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

        private GameLoop gameLoop;
        #endregion

        #region Methods

        private void Start()
        {
            _collider = GetComponent<Collider>();
            if (_door == null)
                throw new MissingReferenceException("Please assign a reference to the furnace's door in the inspector :)");
            if (_slot != null)
            {
                _slot.OnGrab.AddListener((meh) => Idk(false));
                _slot.OnDrop.AddListener((meh) => Idk(true));
            }

            gameLoop = ReferenceManager.Get<GameLoop>();
        }

        private void Update()
        {
            if(gameLoop.IsRunning)
                TryCook();
        }

        /// <summary>
        /// Sorry I don't know what I'm doing here. Just trying to get to the end of my day. And it's sunday.
        /// </summary>
        /// <param name="drop"></param>
        private void Idk(bool drop)
        {
            if (_slot.HeldItem is Plate plate)
            {
                plate.SetItemColliders(drop);
            }
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
            if (!_isCooking)
            {
                return;
            }

            if ((_requireClosedDoor && _door.IsOpen) || _slot.IsFree)
            {
                StopCooking();
                return;
            }
            Cook();
        }

        /// Could be refactored using timers.
        /// Cook => Check if it's a plate, if so, start timer 
        /// If plate is grabbed or toggle is called, stop timer.
        /// If timer reaches the end, transform the plate and start a new timer for overcooked
        /// Add a callback to the timer's update, so that we can add feedbacks on how cooked we are (we're pretty cooked, but we're also cooking)
        /// Don't forget to invoke events.
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

        public void Reset()
        {
            if (_slot != null && _slot.HeldItem != null)
            {
                GameSystem.ReferenceManager.Instance.GetManager<ItemManager>().Despawn(_slot.HeldItem as Item);
                _slot.HeldItem = null;
            }
        }

        #endregion
    }
}