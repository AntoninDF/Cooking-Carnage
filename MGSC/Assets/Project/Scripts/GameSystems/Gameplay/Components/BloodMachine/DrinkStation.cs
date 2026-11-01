using Clients;
using Features;
using GameSystem;
using NaughtyAttributes;
using System;
using UnityEngine;

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
    public float ReserveFill01 => BloodQuantity / _maxBloodQuantityDisplay; public float BloodPerDrink => _bloodPerDrink;
    /// <summary>
    /// Modifying the value of <see cref="BloodQuantity"/> will invoke <see cref="OnBloodQuantityChange"/>.
    /// This allows the reserve's visual appearance to be updated automatically when the blood quantity changes.
    /// </summary>
    public float BloodQuantity
    {
        get => _quantity;
        set
        {
            if (value == _quantity) return;
            _quantity = value;
            OnBloodQuantityChange?.Invoke(value);
        }
    }
    public Action<float> OnBloodQuantityChange { get; set; }
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
    private float _quantity = 0f;
    #endregion

    #region Methods
    public void Initialize(GlobalDrinkStation globalDrinkStation)
    {        
        valve    = new F_Valve(globalDrinkStation, this, globalDrinkStation, Type, servingDuration, _timeBeforeLoosingBlood, _bloodFlowPerSecond);

        BindEvents();
        ActualizeReserveValue(ReserveFill01);

        GameLoop.OnAfterReset += OnReset;
        ShiftManager.OnShiftEnded += OnReset;
    }
    void OnReset()
    {
        _empty();
        valve.OnStopFillingDrink?.Invoke();
        valve.OnStopOverflowing?.Invoke();
        valve.OnStopServing?.Invoke();
        _spawnedDrink = null;
        _spawnedDrinkComponent = null;
        valve.IsServing = false;
    }
    public void BindEvents()
    {
        ValveButton.OnInteraction += TryStartServingDrink;
        valve.OnStopFillingDrink      += WhenStopFillingDrink;
        OnBloodQuantityChange += CheckReserve;
    }

    void TryStartServingDrink(InteractionContext ctx)
    {
        if (!CanServeDrink()) return;
            
        TrySpawnDrink(out var drink);
        valve.TryServeDrink(drink);
    }
    private bool TrySpawnDrink(out Drink drink)
    {
        drink = null;
        if (!CanServeDrink()) return false;

        drink = Spawn();
        return true;
        //TimerManager.StartTimer("Prepare to spawn drink : " + DrinkPrefab.name, 0.25f, Spawn);
    }
    public bool CanServeDrink()
    {
        if (_spawnedDrink != null) return false; // Don't spawn anything if the previous drink is still there
        if (DrinkPrefab == null) return false;
        if (DrinkSpawnPoint == null) return false;
        if (_quantity < _bloodPerDrink) return false;
        if (!valve.CanServeDrink()) return false;
        return true;
    }
    private Drink Spawn()
    {
        _spawnedDrink = GameObject.Instantiate(DrinkPrefab, DrinkSpawnPoint);

        if (_spawnedDrink.TryGetComponent(out _spawnedDrinkComponent))
        {
            _spawnedDrinkComponent.Grabbable.IsLocked = true;
            _spawnedDrinkComponent.Grabbable.OnGrab += WhenDrinkGrabbed;
            _spawnedDrinkComponent.SetLiquidLevel(0f);

            return _spawnedDrinkComponent;
        }
        return null;
    }


    /// <summary>Set the drink's liquid level </summary>
    public void ActualizeDrinkValue(float ratio01) => _spawnedDrinkComponent?.SetLiquidLevel(ratio01);

    public void ActualizeReserveValue(float ratio01)
    {
        
        ratio01 = Mathf.Clamp01(ratio01);
        MaterialPropertyBlock reserveBlock = new();
        _reserveRenderer.GetPropertyBlock(reserveBlock);
        reserveBlock.SetFloat("_LiquidLevel", ratio01);
        _reserveRenderer.SetPropertyBlock(reserveBlock);
    }
        
    void WhenStopFillingDrink()
    {
        if (_spawnedDrinkComponent == null) return;
        _spawnedDrinkComponent.Grabbable.IsLocked = false;
        _spawnedDrinkComponent.SetLiquidLevel(1f);
    }

    private void WhenDrinkGrabbed()
    {
        if (_spawnedDrinkComponent != null)
            _spawnedDrinkComponent.Grabbable.OnGrab -= WhenDrinkGrabbed;

        valve.WhenDrinkGrabbed();

        _spawnedDrink = null;
        _spawnedDrinkComponent = null;
    }
               
    private void CheckReserve(float targetValue) => ActualizeReserveValue(ReserveFill01);

    [Button("Fill")] void _fill() => BloodQuantity = 150;
    [Button("Empty")] void _empty() => BloodQuantity = 0;

    #endregion

}