using NUnit.Framework;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// Well, it's a furnace.
    /// </summary>
    public class Furnace : MultiSlotContainer
    {
        #region Properties

        public UnityEvent OnFailedStartAttempt => _onFailedStartAttempt;
        public UnityEvent OnStartCooking => _onStartCooking;
        public UnityEvent OnStopCooking => _onStopCooking;
        public bool IsCooking => _isCooking;
        public float Strength => _strength;
        #endregion

        #region Fields
        [Header("Furnace references")]
        [Tooltip("The furnace needs a reference to it's own door to ensure it's closed while cooking :)")]
        [SerializeField] private Door _door;
        [Space]
        [Header("Furnace parameters")]
        [Tooltip("Speed modifier for the furnace")]
        [SerializeField] private float _strength = 1.0f;
        [Space]
        [Header("Furnace events")]
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
            Cook();
        }

        public void ToggleCooking()
        {
            if (_isCooking)
            {
                StopCooking();
                return;
            }
            if (_door.IsOpen || Slots.Count == 0)
            {
                OnFailedStartAttempt?.Invoke();
                return;
            }
            _isCooking = true;
            OnStartCooking?.Invoke();
        }

        private void Cook()
        {
            if (!_isCooking) return;

            if (_door.IsOpen)
            {
                StopCooking();
            }

            foreach (var furnaceSlot in Slots)
            {
                if (furnaceSlot == null) continue;
                if (furnaceSlot.IsEmpty) continue;

                MultiSlotContainer tray = null;
                if (!furnaceSlot.HeldItem.transform.gameObject.HasComponent<MultiSlotContainer>(ref tray)) continue;

                foreach (var traySlot in tray.Slots)
                {
                    if (traySlot == null) continue;
                    if (traySlot.IsEmpty) continue;
                    if (traySlot.HeldItem is not ICookable) continue; // What happens when a non-cookable object goes into the oven ?
                    ICookable cookItem = traySlot.HeldItem as ICookable;
                    cookItem.Cook(Time.deltaTime * _strength);
                }
            }
        }

        private void StopCooking()
        {
            _isCooking = false;
            OnStopCooking?.Invoke();
        }

        #endregion
    }

}