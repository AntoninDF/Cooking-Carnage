using System;
using Controller;
using GameSystem;
using UI;
using UnityEngine;
using UnityEngine.Events;


namespace Cooking
{


    public class Blender : MonoBehaviour, IInteractable, IWatchable
    {
        #region Properties
        public float CutDuration { get => _cutDuration; set { _cutDuration = value; } }
        public UnityEvent OnInteract => _onInteract;
        public UnityEvent OnItemCut => _onItemCut;

        public Collider Collider { get; set; }

        public PlayerInterface Player { get; set; }

        public bool IsLookedAt => Player.LookTarget == gameObject;
        public UnityEvent OnLookAt { get; }
        public UnityEvent OnStopLookAt { get; }

        #endregion

        #region Fields
        [Header("Parameters")]
        [SerializeField]
        private float _cutDuration = 2.5f;

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


        // private fields
        TimerManager _timerManager;
        private bool _wasLookedAtThisFrame;
        #endregion


        #region Methods

        private void Update()
        {
            UpdateUI();
        }

        private void Start()
        {
            if (_slotContainer == null)
                _slotContainer = GetComponent<SlotContainer>();
            if (_slotContainer == null)
                throw new MissingReferenceException("[Blender] : Missing reference for the slot container. Please assign it in the inspector");

            _slotContainer.SetFilter(new Filter(EFilterCondition.MustBe, EItemType.Ingredient));
            _slotContainer.OnDrop.AddListener(StopCutting);
            _timerManager = ReferenceManager.Instance.GetManager<TimerManager>();
            if (_timerManager == null)
                throw new MissingReferenceException("[Blender] : No timer manager found");
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (_slotContainer != null && _slotContainer.HeldItem != null)
            {
                if (_slotContainer.HeldItem is Ingredient ingredient)
                {
                    if (!ingredient.IsCut)
                    {
                        _timerManager.StartTimer("Cutting", _cutDuration, () => TryCutIngredient(ingredient));
                        _onInteract?.Invoke();
                    }
                    else
                    {
                        _onTryToCutAlreadyCutIngredient?.Invoke();
                    }
                }
            }
        }

        private void TryCutIngredient(Ingredient ingredient)
        {
            ingredient.Cut();
            _onItemCut?.Invoke();
        }

        public void StopCutting()
        {
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
            if (Player == null || !IsLookedAt)
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
                ReferenceManager.Instance.GetManager<ItemManager>().Despawn(_slotContainer.HeldItem as Ingredient);
                _slotContainer.HeldItem = null;
            }
        }

        #endregion
    }

}