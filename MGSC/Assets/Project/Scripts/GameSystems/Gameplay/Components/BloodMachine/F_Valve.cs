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
        public bool IsServing => isServing;
        public Timer ServingTimer => TimerManager.GetTimer(FirstTimerName);
        public Timer OverflowingTimer => TimerManager.GetTimer(SecondTimerName);
       
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

        private string FirstTimerName  => _associedClientType + " : first timer launched";
        private string SecondTimerName => _associedClientType + " : wait before loosing Blood";

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

        /// <summary>
        /// This method name is misleading, since <see cref="TryGrabDrink"/> is invoked when the drink is grabbed, so there is no "try". 
        /// There is just "do".
        /// https://i.pinimg.com/originals/a5/91/c2/a591c21d89b1215f87f9c8db58a8dc62.jpg
        /// </summary>
        public void TryGrabDrink()
        {
            //if (!isServing) return; // We should be able to take the drink even if the machine stopped serving because it was out of blood
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

        /// <summary>
        /// This coroutine pours blood in the drink for a given duration, and won't stop until it's done.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// This method starts the grace delay between the cup being full and the blood overflowing
        /// </summary>
        public void WaitForPlayerToRetrieve()
        {
            DrinkReady?.Invoke();
            TimerManager.StartTimer(SecondTimerName, _secondTimerTime, LooseBlood);
        }

        /// <summary>
        /// Start overflowing blood without filling up the drink
        /// </summary>
        public void LooseBlood()
        {
            StartLoosingBlood?.Invoke();
            _station.StartCoroutine(DecreaseBlood());
        }

        /// <summary>
        /// This coroutine is invoked when the drink starts overflowing, after a grace delay
        /// </summary>
        /// <returns></returns>
        private IEnumerator DecreaseBlood()
        {
            while (isServing && _reserve.GetDrinkQuantity(_associedClientType) > 0)
            {
                float delta = _bloodDecreaseOverTime * Time.deltaTime;
                _reserve.RemoveBlood(_associedClientType, delta);
                _drinkStation.ActualizeReserveValue(_reserve.GetReserveRatio(_associedClientType));

                yield return null;
            }
            // We exit the loop when the drink is taken or when there isn't any blood yet

            // This shouldn't be invoked here since we don't know if the drink was taken (and if it was, this event is already invoked.
            // But it makes sure that the feedbacks stop anyway. If feedbacks were to change, this should be changed too
            OnDrinkTaked?.Invoke(); 
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