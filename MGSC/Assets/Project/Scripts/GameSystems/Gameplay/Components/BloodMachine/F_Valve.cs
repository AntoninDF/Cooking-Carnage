using System;
using System.Collections;
using Clients;
using Features;
using GameSystem;
using UnityEngine;
using Upgrades;

/// <summary>
/// Handles the pouring of blood, the related timers, and blood consumption
/// </summary>
public class F_Valve : Feature, IUpgradable<DrinkServingSpeedUpgrade>
{
    public EClientType AssociedClientType => _associedClientType;
       
    public Action OnStartFillingDrink   { get; set; } // Invoked if the drink starts filling up and can contain more blood. This would be equivalent to "OnStartServing"
    public Action OnStopFillingDrink        { get; set; } // Invoked when the drink is full
    public Action OnStartOverflowing { get; set; } // Invoked when the drink starts receiving more blood than it can contain
    public Action OnStopOverflowing { get; set; } // Invoked when the drink stops receiving more blood than it can contain
    public Action OnStopServing { get; set; } // Invoked whenever we stop serving, wether we started overflowing or not
    public bool IsServing
    {
        get => _isServing;
        set
        {
            if (value == _isServing) return;
            if (!value) OnStopServing?.Invoke();
            _isServing = value;
        }
    }
    public Timer ServingTimer => TimerManager.GetTimer(FirstTimerName);
    public Timer OverflowingTimer => TimerManager.GetTimer(SecondTimerName);
       
    private EClientType       _associedClientType;
    private DrinkStation      _drinkStation;
    private GlobalDrinkStation _station;

    private Drink _pendingDrink;
    private bool hasPendingDrink => _pendingDrink != null;
    private bool _isServing;

    private float _bloodDecreaseOverTime;
    private float _startServiceDuration;
    private float _secondTimerTime;
    private float _serviceDuration;
    private float _bloodQuantityBeforeServiceStart;

    private string FirstTimerName  => _associedClientType + " : first timer launched";
    private string SecondTimerName => _associedClientType + " : wait before loosing Blood";

    public F_Valve(IFeatureCollection owner,
        DrinkStation        drinkStation,
        GlobalDrinkStation  globalDrinkStation,
        EClientType         clientType, 
        float               startServiceDuration, 
        float               secondTimerTime, 
        float               bloodDecreaseOverTime) : base(owner)
    {
        _drinkStation       = drinkStation;
        _station            = globalDrinkStation;
        _associedClientType = clientType;

        _startServiceDuration  = startServiceDuration;
        _serviceDuration       = startServiceDuration;
        _secondTimerTime       = secondTimerTime;
        _bloodDecreaseOverTime = bloodDecreaseOverTime;
    }
       


    public bool TryServeDrink(Drink drinkToServe)
    {
        Debug.Log("Valve.TryServeDrink invoked");
        if (!CanServeDrink()) return false;

        Debug.Log("Valve.TryServeDrink has passed checks");
        IsServing = true;
        _pendingDrink = drinkToServe;
        OnStartFillingDrink?.Invoke();
        _bloodQuantityBeforeServiceStart = _drinkStation.BloodQuantity;
        TimerManager.StartGameplayTimer(FirstTimerName, _serviceDuration, WaitForPlayerToRetrieveDrink, ProcessDrinkPouring);

        return true;
    }

    public bool CanServeDrink()
    {
        if (!Globals.Authorizations.AllowMakingDrinks) return false;
        if (hasPendingDrink) return false; // Une boisson est déjà en attente d'être récupérée
        if (IsServing) return false; // Can't serve a new drink if we're already serving one
        return true;
    }

    /// <summary>This method is called on the update of the timer that pours blood in the drink. </summary>
    private void ProcessDrinkPouring(float elapsed)
    {
        Debug.Log("Valve.ProcessDrinkPouring");
        float bloodPerDrink  = _drinkStation.BloodPerDrink;
        float t = Mathf.Clamp01(elapsed / _serviceDuration);

        _drinkStation.BloodQuantity = _bloodQuantityBeforeServiceStart - bloodPerDrink * t;
        _pendingDrink.SetLiquidLevel(t);
    }

    /// <summary>
    /// This method starts the grace delay between the cup being full and the blood overflowing
    /// </summary>
    public void WaitForPlayerToRetrieveDrink()
    {
        Debug.Log("Valve.WaitForPlayerToRetrieveDrink invoked");
        OnStopFillingDrink?.Invoke();
        TimerManager.StartGameplayTimer(SecondTimerName, _secondTimerTime, StartOverflowing);
    }

    /// <summary>
    /// Start overflowing blood without filling up the drink
    /// </summary>
    public void StartOverflowing()
    {
        Debug.Log("Valve.StartOverflowing invoked");
        OnStartOverflowing?.Invoke();
        TimerManager.StartGameplayTimer("Start overflowing drink station " + _associedClientType, float.MaxValue, null, ProcessDecreasingBlood);
    }

    /// <summary>
    /// This method is invoked each frame during which the drink overflows, after a grace delay
    /// </summary>
    private void ProcessDecreasingBlood(float temp)
    {
        float delta = _bloodDecreaseOverTime * Time.deltaTime;
        _drinkStation.BloodQuantity -= delta;

        Debug.Log("Valve.ProcessDecreasingBlood invoked");
        if (IsServing && _drinkStation.BloodQuantity > 0) return;
            
        // We exit the loop when the drink is taken or when there isn't any blood yet
        OnStopOverflowing?.Invoke();
        IsServing = false;
        TimerManager.StopTimer("Start overflowing drink station " + _associedClientType);
    }
       
    public void WhenDrinkGrabbed()
    {
        Debug.Log("Valve.WhenDrinkGrabbed invoked");
        IsServing = false;
        _pendingDrink = null;

        if (TimerManager.IsTimerRunning(SecondTimerName))
            TimerManager.StopTimer(SecondTimerName);
    }

    #region Upgrade

    public void OnUpgradeInit(DrinkServingSpeedUpgrade upgrade)
    {
        _serviceDuration = upgrade.GetServiceDuration(_startServiceDuration);
    }

    public void OnUpgraded(DrinkServingSpeedUpgrade upgrade)
    {
        _serviceDuration = upgrade.GetServiceDuration(_startServiceDuration);
    }

    #endregion

     
}