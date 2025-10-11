using NUnit.Framework;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// Well, it's a furnace.
    /// </summary>
    public class Furnace : MultiSlotContainer
    {
        #region Properties

        public Action OnFailedStartAttempt {  get; set; } // Invoked when the player tries to start cooking, but it fails. How ironic
        public Action OnStartCooking { get; set; }
        public Action OnStopCooking { get; set; }
        public bool IsCooking => _isCooking;
        public float Strength => _strength;
        #endregion

        #region Fields
        [Header("Furnace references")]
        [Tooltip("When this interactable will be interacted with, it will start cooking")]
        [SerializeField] private Interactable _button;
        [Tooltip("The furnace needs a reference to it's own door to ensure it's closed while cooking :)")]
        [SerializeField] private Door _door;
        [SerializeField] private ParticleSystem _onStartCooking;
        [SerializeField] private ParticleSystem _onStopCooking;
        [SerializeField] private ParticleSystem _onFailedStartCooking;
        [SerializeField] private GameObject _cookingIndicator;
        [Space]
        [Header("Furnace parameters")]
        [Tooltip("Speed modifier for the furnace")]
        [SerializeField] private float _strength = 1.0f;

        private bool _isCooking = false;
        #endregion

        #region Methods

        private void Start()
        {
            if (_door == null)
                throw new MissingReferenceException("Please assign a reference to the furnace's door in the inspector :)");
            if (_button == null)
                throw new MissingReferenceException("Please assign a reference to the furnace's start button in the inspector :)");
            if (_cookingIndicator == null)
                throw new MissingReferenceException("Please assign a reference to the furnace's cooking indicator in the inspector :)");

            _button.OnInteract += ToggleCooking;

            // Feedback, to do again better later
            OnStartCooking += () => _onStartCooking?.Play();
            OnStopCooking += () => _onStopCooking?.Play();
            OnFailedStartAttempt += () => _onFailedStartCooking?.Play();
            OnStartCooking += () => _cookingIndicator?.SetActive(true);
            OnStopCooking += () => _cookingIndicator?.SetActive(false);

            _cookingIndicator.SetActive(false);
        }

        private void Update()
        {
            Cook();
        }

        private void ToggleCooking()
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