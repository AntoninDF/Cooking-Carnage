using System;
using Clients;
using Features;
using GameSystem;
using NaughtyAttributes;
using Project.Scripts.GameSystems.Gameplay.Components.Drinks;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Scripts.BloodMachine
{
    /// <summary>
    /// Handle drink service for one given blood/client type
    /// </summary>
	[Serializable]
    public class DrinkStation : MonoBehaviour, IFeatureCollection
    {
        #region Properties
        public bool IsServing => valve.IsServing;
        public EClientType Type { get => _type; set => _type = value; }
        public GameObject DrinkPrefab { get => _drinkPrefab; set => _drinkPrefab = value; }
        public Transform DrinkSpawnPoint { get => _drinkSpawnPoint; set => _drinkSpawnPoint = value; }
        public float ReserveFill01 => reserve.BloodQuantity / _maxBloodQuantityDisplay;
        public FeatureCollection Collection { get; } = new();
        #endregion

        #region Fields
        // SERIALIZED FIELDS
        [Header("Parameters")]
        [Tooltip("What kind of blood does this station use/serve")]
        [SerializeField] private EClientType       _type;
        [Tooltip("Unit is centiliters")]
        [SerializeField] private int _bloodPerDrink = 30;
        [Tooltip("Unit is centiliters per second")]
        [SerializeField] private float _bloodFlowPerSecond = 6f;// This is the delay between each partial fillup of a drink (→ 3cL per 0.5s, this is the 0.5s)
        [Tooltip("Unit is seconds")]
        [SerializeField] private float _timeBeforeLoosingBlood = 2f;
        [Min(1f), Tooltip("Since the blood tanks visually have a maximum, this is how much blood is needed to make them appear full. In practice, they have infinite capacity")]
        [SerializeField] private int _maxBloodQuantityDisplay = 150;
        float servingDuration => _bloodPerDrink / _bloodFlowPerSecond;

        [Header("References")]
        [SerializeField] private InteractionButton ValveButton;
        [SerializeField] private  GameObject _drinkPrefab;
        [SerializeField] private Transform  _drinkSpawnPoint;
        [SerializeField] private Renderer _reserveRenderer;
        [SerializeField] private Animator _animator;
        [SerializeField] private AnimationClip         _startServeClip;
                
        // NON-SERIALIZED FIELDS
        private GameObject _spawnedDrink;
        private Drink      _spawnedDrinkComponent;

        public F_Valve         valve;
        public F_BloodReserve  reserve;
        #endregion

        #region Methods
        public void Initialize(GlobalDrinkStation globalDrinkStation)
        {        
            reserve = new(this, _bloodPerDrink);
            valve    = new F_Valve(globalDrinkStation, this, globalDrinkStation, reserve, Type, servingDuration, _timeBeforeLoosingBlood, _bloodFlowPerSecond);

            BindEvents();
            ActualizeReserveValue(ReserveFill01);
        }
        public void BindEvents()
        {
            ValveButton.OnInteraction += TryStartServingDrink;
            valve.OnStopFillingDrink      += WhenDrinkReady;
            reserve.OnBloodQuantityChange += CheckReserve;
        }

        void TryStartServingDrink(InteractionContext ctx) => valve.TryServeDrink();

        /// <summary>Set the drink's liquid level </summary>
        public void ActualizeDrinkValue(float ratio01) => _spawnedDrinkComponent?.SetLiquidLevel(ratio01);

        public void ActualizeReserveValue(float ratio01)
        {
            Debug.Log("DrinkStation.ActualizeReserveValue invoked");
            ratio01 = Mathf.Clamp01(ratio01);
            MaterialPropertyBlock reserveBlock = new();
            _reserveRenderer.GetPropertyBlock(reserveBlock);
            reserveBlock.SetFloat("_LiquidLevel", ratio01);
            _reserveRenderer.SetPropertyBlock(reserveBlock);
        }
        
        void WhenDrinkReady() => TrySpawnDrink();
        private void TrySpawnDrink()
        {
            Debug.Log("DrinkStation.TrySpawnDrink invoked");
            if (_spawnedDrink != null) return; // Don't spawn anything if the previous drink is still there
            if (DrinkPrefab == null) return;
            if (DrinkSpawnPoint == null) return;

            TimerManager.StartTimer("Prepare to spawn drink : " + DrinkPrefab.name, 0.25f, Spawn);
        }

        private void Spawn()
        {
            Debug.Log("DrinkStation.Spawn invoked");
            _spawnedDrink = GameObject.Instantiate(DrinkPrefab, DrinkSpawnPoint.position, DrinkSpawnPoint.rotation);
            valve.SetPendingDrink(true);

            if (_spawnedDrink.TryGetComponent(out _spawnedDrinkComponent))
            {
                _spawnedDrinkComponent.Grabbable.OnGrab += WhenDrinkGrabbed;
                _spawnedDrinkComponent.SetLiquidLevel(1f);
            }
        }


        private void WhenDrinkGrabbed()
        {
            Debug.Log("DrinkStation.WhenDrinkGrabbed invoked");
            if (_spawnedDrinkComponent != null)
                _spawnedDrinkComponent.Grabbable.OnGrab -= WhenDrinkGrabbed;

            valve.WhenDrinkGrabbed();

            _spawnedDrink = null;
            _spawnedDrinkComponent = null;
            valve.SetPendingDrink(false);
        }
               
        private void CheckReserve(float targetValue) => ActualizeReserveValue(ReserveFill01);

        [Button("Fill")] void _fill() => reserve.BloodQuantity = 150;
        [Button("Empty")] void _empty() => reserve.BloodQuantity = 0;

        #endregion

    }
}