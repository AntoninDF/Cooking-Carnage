using System;
using System.Collections;
using Clients;
using Features;
using GameSystem;
using UnityEngine;
using Upgrades;

namespace Project.Scripts.BloodMachine
{
    public class F_Valve : Feature, IUpgradable<DrinkServingSpeedUpgrade>
    {
       public EClientType AssociedClientType => _associedClientType;
       
       public Action StartServeDrink   { get; set; }
       public Action DrinkReady        { get; set; }
       public Action StartLoosingBlood { get; set; }
       public Action OnDrinkTaked      { get; set; }
       
       private EClientType       _associedClientType;
       private F_BloodReserve    _reserve;
       private DrinkStation      _drinkStation;
       private GlobalDrinkStation _station;
       
       private bool _hasPendingDrink;
       private bool isServing;

       private float _bloodDecreaseOverTime;
       private float _startServiceDuration;
       private float _secondTimerTime;

       private float _serviceDuration;

       private string FirstTimerName  => _associedClientType + "first timer launched";
       private string SecondTimerName => _associedClientType + "wait before loosing Blood";

       public F_Valve(IFeatureCollection owner,
          DrinkStation        drinkStation,
          GlobalDrinkStation  globalDrinkStation,
          F_BloodReserve      reserve, 
          EClientType         clientType, 
          float               startServiceDuration, 
          float               secondTimerTime, 
          float               bloodDecreaseOverTime) : base(owner)
       {
          _reserve            = reserve;
          _drinkStation       = drinkStation;
          _station            = globalDrinkStation;
          _associedClientType = clientType;

          _startServiceDuration  = startServiceDuration;
          _serviceDuration       = startServiceDuration;
          _secondTimerTime       = secondTimerTime;
          _bloodDecreaseOverTime = bloodDecreaseOverTime;
       }
       

       public void SetPendingDrink(bool value) => _hasPendingDrink = value;

       public void TryServeDrink()
       {
          if (!Globals.Gameplay.AllowBlending)
          {
             //Debug.Log("[F_Valve] is not allowed to serve, the blood machine is not ulocked");
             return;
          }
          if (_hasPendingDrink)
          {
             //Debug.Log("[F_Valve] Une boisson est déjà en attente d'être récupérée");
             return;
          }

          if (_reserve.CanServeDrink(_associedClientType) && !isServing)
          {
             isServing = true;
             StartServeDrink?.Invoke();
             TimerManager.StartTimer(FirstTimerName, _serviceDuration, WaitForPlayerToRetrieve);
             _station.StartCoroutine(PourDrinkRoutine());
          }
          else
          {
             //Debug.Log($"[F_Valve] the drink cant be serve, CanServeDrink = {_reserve.CanServeDrink(_associedClientType)} & isServing = {isServing}");
          }
       }

       public void TryGrabbDrink()
       {
          if (!isServing) return;
          if (TimerManager.IsTimerRunning(FirstTimerName))
          {
             //Debug.Log("[F_Valve] votre boisson est occupée");
             return;
          }
          DrinkTaken();
       }

       // public void TryServeDrink()
       // {
       //    if (_reserve.CanServeDrink(_associedClientType) && !isServing)
       //    {
       //       isServing = true;
       //       StartServeDrink?.Invoke();
       //       TimerManager.StartTimer(FirstTimerName, _firstTimerTime, WaitForPlayerToRetrieve);
       //       _station.StartCoroutine(PourDrinkRoutine());
       //    }
       // }

       private IEnumerator PourDrinkRoutine()
       {
          float elapsed        = 0f;
          float bloodPerDrink  = _reserve.BloodPerDrink;

          while (elapsed < _startServiceDuration)
          {
             elapsed += Time.deltaTime;
             float t = Mathf.Clamp01(elapsed / _startServiceDuration);

             _drinkStation.ActualizeDrinkValue(t);

             float delta = (bloodPerDrink / _startServiceDuration) * Time.deltaTime;
             _reserve.RemoveBlood(_associedClientType, delta);
             _drinkStation.ActualizeReserveValue(_reserve.GetReserveRatio(_associedClientType));

             yield return null;
          }

          _drinkStation.ActualizeDrinkValue(1f);
       }

       public void WaitForPlayerToRetrieve()
       {
          DrinkReady?.Invoke();
          TimerManager.StartTimer(SecondTimerName, _secondTimerTime, LooseBlood);
       }

       public void LooseBlood()
       {
          StartLoosingBlood?.Invoke();
          _station.StartCoroutine(DecreaseBlood());
       }

       private IEnumerator DecreaseBlood()
       {
          while (isServing && _reserve.GetDrinkQuantity(_associedClientType) > 0)
          {
             float delta = _bloodDecreaseOverTime * Time.deltaTime;
             _reserve.RemoveBlood(_associedClientType, delta);
             _drinkStation.ActualizeReserveValue(_reserve.GetReserveRatio(_associedClientType));

             yield return null;
          }

          isServing = false;
       }
       
       public void DrinkTaken()
       {
          isServing = false;
          OnDrinkTaked?.Invoke();
          if(TimerManager.IsTimerRunning(SecondTimerName))
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
}