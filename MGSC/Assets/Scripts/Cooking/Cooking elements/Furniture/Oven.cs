using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    public class Oven : MonoBehaviour
    {
        #region Properties
        public UnityEvent OnStartCooking => _onStartCooking;
        public UnityEvent OnStopCooking => _onStopCooking;
        public UnityEvent OnFailedStartAttempt => _onFailedStartAttempt;
        public bool IsCooking => _isCooking;
        public float Strength => _strength;

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
        [Space]
        [Header("Oven events")]
        [Tooltip("Invoked when the furnace starts cooking")]
        [SerializeField] private UnityEvent _onStartCooking;
        [Tooltip("Invoked when the furnace stops cooking")]
        [SerializeField] private UnityEvent _onStopCooking;
        [Tooltip("Invoked when the player tries to start the furnace, but it cannot be started (if the door is opened for example)")]
        [SerializeField] private UnityEvent _onFailedStartAttempt;

        private bool _isCooking = false;
        #endregion

        #region Methods

        private void Start()
        {
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
            if (_door.IsOpen || _slot.IsFree)
            {
                OnFailedStartAttempt?.Invoke();
                return;
            }
            StartCooking();
        }

        private void TryCook()
        {
            if (!_isCooking) return;

            if (_door.IsOpen || _slot.IsFree)
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
        }

        public void StartCooking()
        {
            _isCooking = true;
            OnStartCooking?.Invoke();
        }

        public void StopCooking()
        {
            _isCooking = false;
            ICookable cookItem = _slot.HeldItem as ICookable;
            if (cookItem == null) return;
            cookItem.StopCooking();
            OnStopCooking?.Invoke();
        }

        #endregion
    }
}