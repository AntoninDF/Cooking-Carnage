using System;
using System.Collections;
using Clients;
using Features;
using GameSystem;
using UnityEngine;

namespace Project.Scripts.GameSystems.Gameplay.Components.BloodMachine
{
    public class F_Valve : Feature
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
       
       private bool isServing;

       private float _bloodDecreaseOverTime;
       private float _firstTimerTime;
       private float _secondTimerTime;

       private string FirstTimerName  => _associedClientType + "first timer launched";
       private string SecondTimerName => _associedClientType + "wait before loosing Blood";

       public F_Valve(IFeatureCollection owner,
          DrinkStation        drinkStation,
          GlobalDrinkStation  globalDrinkStation,
          F_BloodReserve      reserve, 
          EClientType         clientType, 
          float               firstTimerTime, 
          float               secondTimerTime, 
          float               bloodDecreaseOverTime) : base(owner)
       {
          _reserve            = reserve;
          _drinkStation       = drinkStation;
          _station            = globalDrinkStation;
          _associedClientType = clientType;

          _firstTimerTime        = firstTimerTime;
          _secondTimerTime       = secondTimerTime;
          _bloodDecreaseOverTime = bloodDecreaseOverTime;
       }

       public void TryServeDrink()
       {
          if (_reserve.CanServeDrink(_associedClientType) && !isServing)
          {
             isServing = true;
             StartServeDrink?.Invoke();
             TimerManager.StartTimer(FirstTimerName, _firstTimerTime, WaitForPlayerToRetrieve);
             _station.StartCoroutine(PourDrinkRoutine());
          }
       }

       // Chrono 1 : le verre se remplit progressivement pendant que la réserve baisse en parallèle
       private IEnumerator PourDrinkRoutine()
       {
          float elapsed        = 0f;
          float bloodPerDrink  = _reserve.BloodPerDrink;

          while (elapsed < _firstTimerTime)
          {
             elapsed += Time.deltaTime;
             float t = Mathf.Clamp01(elapsed / _firstTimerTime);

             _drinkStation.ActualizeDrinkValue(t);

             float delta = (bloodPerDrink / _firstTimerTime) * Time.deltaTime;
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

       public void TryGrabbDrink()
       {
          if (TimerManager.IsTimerRunning(FirstTimerName))
          {
             Debug.Log("[F_Valve] votre boisson est occupée");
             return;
          }
          else
          {
             DrinkTaken();
          }
       }
       
       public void DrinkTaken()
       {
          isServing = false;
          OnDrinkTaked?.Invoke();
          if(TimerManager.IsTimerRunning(SecondTimerName))
             TimerManager.StopTimer(SecondTimerName);
       }
    }
}