using Controller;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// Base class for any interactable object (that isn't an ingredient)
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Interactable : MonoBehaviour, IInteractable, IWatchable
    {
        #region Properties
        public UnityEvent OnInteract => _onInteract;
        public UnityEvent OnLookAt => _onLookAt;
        public UnityEvent OnStopLookAt => _onStopLookAt;

        public PlayerInterface Player { get; set; }
        public bool IsLookedAt => Player.LookTarget == gameObject;
        public Collider Collider => _collider;

        #endregion

        #region Fields
        [Header("Interactable references")]
        [Tooltip("Reference the canvas or panel game object that must show when the interactable is hovered.")]
        [SerializeField] private GameObject _tooltip;
        [Tooltip("Invoked when the interactable is interacted with")]
        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private UnityEvent _onLookAt;
        [SerializeField] private UnityEvent _onStopLookAt;

        private Collider _collider;
        private bool _wasLookedAtThisFrame;
        #endregion

        #region Methods

        void Start()
        {
            _collider = GetComponent<Collider>();
        }

        void Update()
        {
            UpdateUI();
            CheckIfLookedAt();
        }

        private void CheckIfLookedAt()
        {
            bool lookedAt = (Player != null && Player.LookTarget == this.gameObject);
            if (lookedAt && !_wasLookedAtThisFrame)
                _onLookAt?.Invoke();
            else if (!lookedAt && _wasLookedAtThisFrame)
                _onStopLookAt?.Invoke();
            _wasLookedAtThisFrame = lookedAt;
        }
        /// <summary>
        /// Simply invokes OnInteract
        /// </summary>
        /// <param name="interactor"></param>
        public virtual void Interact(PlayerInteractor interactor)
        {
            OnInteract?.Invoke();
        }

        /// <summary>
        /// Tell this IWatchable that it is being looked at
        /// </summary>
        /// <param name="interactor"></param>
        public void LookAt(PlayerInteractor interactor)
        {
            Player = interactor.Player;
        }

        private void UpdateUI()
        {
            if (_tooltip == null)
                throw new MissingReferenceException("Please reference the tooltip gameobject in the inspector of " + name);
            if (Player == null || !IsLookedAt)
            {
                _tooltip.SetActive(false);
                return;
            }
            _tooltip.SetActive(true);
        }
        #endregion
    }

}