using System;
using Clients;
using Features;
using GameSystem;
using Project.Scripts.GameSystems.Gameplay.Components.Drinks;
using UnityEngine;

namespace Project.Scripts.BloodMachine
{
	[Serializable]
    public class DrinkStation : IFeatureCollection
    {
        public FeatureCollection Collection { get; } = new();

        [SerializeField] private EClientType       Type;
        [SerializeField] private InteractionButton ValveButton;

        [Header("Drink")]
        [SerializeField] private GameObject _drinkPrefab;
        [SerializeField] private Transform  _drinkSpawnPoint;

        [Header("Liquid")]
        [SerializeField] private Renderer _reserveRenderer;
        [SerializeField] private int      _reserveMaterialIndex;

        [Header("Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] private AnimationClip         _startServeClip;


        private MaterialPropertyBlock _drinkBlock;
        private MaterialPropertyBlock _reserveBlock;

        private GameObject _spawnedDrink;
        private Drink      _spawnedDrinkComponent;

        public F_Valve         valve;
        public F_BloodReserve  reserve;
        public F_ValveAnimation animator;

        private float _firstTimerTime;
        
        private const float MinCuveLiquidLevel = 0.25f;
        private const float MaxCuveLiquidLevel = 0.75f;
        
        private const float MinDrinkLiquidLevel = 0.5f;
        private const float MaxDrinkLiquidLevel = 0.619f;

        private  float RemapLiquidLevel(float ratio01, float minLevel, float maxLevel) =>
            Mathf.Lerp(minLevel, maxLevel, Mathf.Clamp01(ratio01));
        
        public void Initialize(
            IFeatureCollection owner,
            GlobalDrinkStation globalDrinkStation,
            F_BloodReserve     reserve,
            float              firstTimerTime,
            float              secondTimerTime,
            float              bloodDecreaseOverTime)
        {
            _drinkBlock   = new MaterialPropertyBlock();
            _reserveBlock = new MaterialPropertyBlock();
        
            this.reserve = reserve;
            valve    = new F_Valve(owner, this, globalDrinkStation, reserve, Type, firstTimerTime, secondTimerTime, bloodDecreaseOverTime);
            animator = new F_ValveAnimation(this, _animator, _startServeClip, firstTimerTime); 
            
        }

        public void ActualizeDrinkValue(float ratio01)
        {
            _spawnedDrinkComponent?.SetLiquidLevel(RemapLiquidLevel(ratio01, MinDrinkLiquidLevel, MaxDrinkLiquidLevel));
        }

        public void ActualizeReserveValue(float ratio01)
        {
            _reserveRenderer.GetPropertyBlock(_reserveBlock, _reserveMaterialIndex);
            _reserveBlock.SetFloat("_LiquidLevel", RemapLiquidLevel(ratio01, MinCuveLiquidLevel, MaxCuveLiquidLevel));
            _reserveRenderer.SetPropertyBlock(_reserveBlock, _reserveMaterialIndex);
        }
        
        public void BindEvents()
        {
            ValveButton.OnInteraction += _ => valve.TryServeDrink();

            reserve.OnBloodAdded   += CheckReserve;
            reserve.OnBloodRemoved += CheckReserve;

            valve.StartServeDrink += animator.StartServing;
            valve.DrinkReady      += animator.ResetSpeed;   
            valve.DrinkReady      += SpawnDrink;
            valve.OnDrinkTaked    += animator.StopServe;
            valve.OnDrinkTaked    += HandleDrinkTaked;
        }

        private void SpawnDrink()
        {
            if (_spawnedDrink != null) return;

            if (_drinkPrefab == null || _drinkSpawnPoint == null)
            {
                Debug.LogError($"[DrinkStation] _drinkPrefab ou _drinkSpawnPoint non assigné sur la station (type {Type}) — vérifier l'Inspector.");
                return;
            }

            TimerManager.StartTimer(_drinkPrefab.name + "Station", 0.25f, Spawn);
        }

        private void Spawn()
        {
            _spawnedDrink = GameObject.Instantiate(_drinkPrefab, _drinkSpawnPoint.position, _drinkSpawnPoint.rotation);
            valve.SetPendingDrink(true);

            if (_spawnedDrink.TryGetComponent(out _spawnedDrinkComponent))
            {
                _spawnedDrinkComponent.Grabbable.OnGrab += HandleDrinkGrabbed;
                _spawnedDrinkComponent.SetLiquidLevel(RemapLiquidLevel(1f, MinDrinkLiquidLevel, MaxDrinkLiquidLevel));
            }
            else
            {
                Debug.LogWarning($"[DrinkStation] Le prefab {_drinkPrefab.name} n'a pas de composant Drink");
            }
        }


        private void HandleDrinkGrabbed()
        {
            if (_spawnedDrinkComponent != null)
                _spawnedDrinkComponent.Grabbable.OnGrab -= HandleDrinkGrabbed;

            valve.TryGrabbDrink();
        }

        private void HandleDrinkTaked()
        {
            _spawnedDrink          = null;
            _spawnedDrinkComponent = null;
            valve.SetPendingDrink(false);
        }
        
        private void CheckReserve(EClientType clientType, float targetValue)
        {
            if (clientType == Type)
                ActualizeReserveValue(targetValue);
        }
        
    }
    
    public class DrinkStationFeatures : Feature
    {
        public DrinkStationFeatures(IFeatureCollection owner) : base(owner)
        {
        }
    }
}