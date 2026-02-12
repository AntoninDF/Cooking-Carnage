using System;
using Controller;
using GameSystem;
using UI;
using UnityEngine;
using UnityEngine.Events;


namespace Cooking
{


    public class Blender : MonoBehaviour, IInteractable, IWatchable, ICallbackBroadcaster
    {
        #region Properties
        public float CutDuration { get => _cutDuration; set { _cutDuration = value; } }
        public float DefaultCutDuration { get => _defaultCutDuration; set { _defaultCutDuration = value; } }
        public UnityEvent OnInteract => _onInteract;
        public UnityEvent OnItemCut => _onItemCut;

        public Collider Collider { get; set; }

        public PlayerInterface Player { get; set; }

        public bool IsLookedAt => Player.LookTarget == gameObject;
        public UnityEvent OnLookAt { get; }
        public UnityEvent OnStopLookAt { get; }
        public bool IsLocked { get; set; }
        public bool EnableCallbacks { get; set; }
        public ICallbackBroadcaster Self => this;

        #endregion

        #region Fields
        [Header("Parameters")]
        [SerializeField]
        private float _cutDuration = 2.5f;
        private float _defaultCutDuration;

        [Header("References")]

        [Tooltip("Reference the canvas or panel game object that must show when the ingredient is hovered.")]
        [SerializeField] protected GameObject _tooltip;
        [SerializeField]
        private SlotContainer _slotContainer;

        [Header("Events")]
        [SerializeField]
        private UnityEvent _onInteract;
        [SerializeField]
        private UnityEvent _onItemCut;
        [SerializeField]
        private UnityEvent _onStopCutting;
        [SerializeField]
        private UnityEvent _onTryToCutAlreadyCutIngredient;
        [SerializeField]
        private UnityEvent _onLookAt;
        [SerializeField]
        private UnityEvent _onStopLookAt;

        [SerializeField] private UnityEvent _onFailedInteraction;


        // private fields
        TimerManager _timerManager;
        private bool _wasLookedAtThisFrame;
        private bool _isCutting;
        #endregion


        #region Methods

        private void Update()
        {
            UpdateUI();
        }
        private void Awake()
        {

            _defaultCutDuration = _cutDuration;
        }
        private void Start()
        {
            if (_slotContainer == null && !TryGetComponent(out _slotContainer))
                throw new MissingReferenceException("[Blender] : Missing reference for the slot container. Please assign it in the inspector");

            _slotContainer.SetFilter(new Filter(EFilterCondition.MustBe, EItemType.Ingredient));
            _slotContainer.OnDrop.AddListener(StopCutting);
            Reference.GetManager(out _timerManager);
            if (_timerManager == null)
                throw new MissingReferenceException("[Blender] : No timer manager found");

        }

        public bool RequestInteraction(PlayerInteractor interactor)
        {
            if (
                IsLocked ||
                _slotContainer == null || 
                _slotContainer.HeldItem == null ||
                _slotContainer.HeldItem is not Ingredient ingredient ||
                ingredient.IsCut
                )
            {
                ingredient = _slotContainer.HeldItem as Ingredient;
                Debug.Log("IsLocked : " + IsLocked);
                Debug.Log("SlotContainer : " + (_slotContainer == null ? "null" : "not null"));
                Debug.Log("Held item : " + _slotContainer?.HeldItem);
                if (ingredient != null) Debug.Log("Ingredient is cut : " + (ingredient.IsCut?"yup":"nope") );
                Self.Callback(_onFailedInteraction);
                return false;
            }
            Interact(ingredient);
            return true;
        }

        protected void Interact(Ingredient ingredient)
        {
            _isCutting = true;
            _timerManager.StartTimer("Cutting", _cutDuration, () => TryCutIngredient(ingredient));
            Self.Callback(_onInteract);
        }

        private void TryCutIngredient(Ingredient ingredient)
        {
            if (IsLocked) return;
            ingredient.Cut();
            Self.Callback(_onItemCut);
            _isCutting = false;
        }

        public void StopCutting()
        {
            if (!_isCutting) return;    
            _isCutting = false;
            _timerManager.StopTimer("Cutting");
            _onStopCutting?.Invoke();
        }
        public void StopCutting(IGrabbable item) => StopCutting();

        /// <summary>
        /// Tell this IWatchable that it is being looked at
        /// </summary>
        /// <param name="interactor"></param>
        public void LookAt(PlayerInteractor interactor)
        {
            Player = interactor.Player;
            _onLookAt?.Invoke();

        }
        public void StopLookAt()
        {
            _onStopLookAt?.Invoke();
        }
        protected virtual void UpdateUI()
        {
            if (_tooltip == null)
                throw new MissingReferenceException("Please reference the tooltip gameobject in the inspector of " + name);
            if (Player == null || !IsLookedAt || IsLocked)
            {
                if (_wasLookedAtThisFrame) StopLookAt();
                _wasLookedAtThisFrame = false;
                _tooltip.SetActive(false);
                return;
            }
            _wasLookedAtThisFrame = true;
            _tooltip.SetActive(true);
        }

        public void Reset()
        {
            if (_slotContainer != null && _slotContainer.HeldItem != null)
            {
                if (Reference.TryGetManager(out ItemManager im))
                    im.Despawn(_slotContainer.HeldItem as Ingredient);
                _slotContainer.HeldItem = null;
            }
        }

        #endregion
    }

}