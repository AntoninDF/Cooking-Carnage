using System;
using Controller;
using UI;
using Unity.VisualScripting;
using UnityEngine;

namespace GameSystem
{
    public class ShiftManager : IManager
    {
        #region Properties
        public Action OnShiftFailed { get; set; }
        public Action OnShiftSucceeded { get; set; }
        public Action OnNextShift { get; set; }
        public Action OnShiftEnded { get; set; }
        public int ShiftCount => _shiftCount;
        public float ShiftDuration => _shiftDuration;
        public MoneyInfo CurrentRentPrice => new MoneyInfo(_currentRentPrice);
    
        #endregion

        #region Fields
    
        private DataManager _dataManager;
    
        private int _shiftCount = 0;
        private float _currentRentPrice = 0 ;
        private AnimationCurve _rentPriceCurve;
        private float _shiftDuration = 120;
    
        #endregion

        #region Methods
   
        public ShiftManager()
        {
            Register();
        }
        public ShiftManager(float shiftDuration)
        {
            Register();
            SetShiftDuration(shiftDuration);
        }
        public ShiftManager(float shiftDuration, AnimationCurve rentPriceCurve)
        {
            Register();
            SetShiftDuration(shiftDuration);
            SetRentPriceCurve(rentPriceCurve);
        }

        public void SetShiftCount(int count) => _shiftCount = count;
        public void SetShiftDuration(float duration) => _shiftDuration = duration;
        public void SetRentPriceCurve(AnimationCurve rentPriceCurve) => _rentPriceCurve = rentPriceCurve;
        public void SetRentPrice(float price) => _currentRentPrice = price;
        public float GetCurrentRentPrice() => GetRentPrice(_shiftCount);
        public float GetRentPrice(int shiftCount) => _rentPriceCurve.Evaluate(shiftCount);
        public void UpdateRentPrice() { SetRentPrice(GetCurrentRentPrice()); }

        void LaunchShift(float duration)
        {
            this.Log(LogType.Log, $"Launching new shift for {duration}s");
            
            ReferenceManager.Instance.GetManager<TimerManager>().StartTimer("shiftDuration", duration, () => EndShift(), false);
        }

        void DelayEndShift()
        {
            ReferenceManager.Instance
                .GetManager<TimerManager>()
                .StartTimer("shiftEndDelay", 2, () => EndShift(), false);
        }

        void EndShift()
        {
            var clientManager = ReferenceManager.Instance.GetManager<ClientManager>();
            int activeClients = clientManager.CurrentClients.Count;
            
            this.Log(LogType.Log, $"End of the Shift triggered. Active clients: {activeClients}");

            if (activeClients != 0)
            {
                DelayEndShift();
            }
            else
            {
                OnShiftEnded?.Invoke();

                ReferenceManager.Instance.GetManager<UIManager>().PauseGame();
                ResetAll();

                // Pay rent
                UpdateRentPrice();
                var moneyManager = ReferenceManager.Instance.GetManager<MoneyManager>();
                if (moneyManager.MoneyInfo < _currentRentPrice)
                {
                    this.Log(LogType.Log, $"Not enough money ({moneyManager.MoneyInfo}) to pay the rent ({_currentRentPrice}€)");
                    OnShiftFailed?.Invoke();
                    return;
                }
                moneyManager.AddMoney(-_currentRentPrice);
                OnShiftSucceeded?.Invoke();

            }
        }

        private void ResetAll()
        {
            this.Log("Reset everything for the next shift");
            ReferenceManager.Instance.GetManager<ItemManager>().StoreAllActive();

            foreach (var blender in ReferenceManager.Instance.GetElementsOfType<Cooking.Blender>())
            {
                blender.Reset();
            }

            foreach (var oven in ReferenceManager.Instance.GetElementsOfType<Cooking.Oven>())
            {
                oven.Reset();
            }
            ReferenceManager.Instance.GetElementOfType<PlayerController>().transform.position = ReferenceManager.Instance.GetElementOfType<PlayerStart>().transform.position;
            ReferenceManager.Instance.GetElementOfType<PlayerController>().transform.rotation = ReferenceManager.Instance.GetElementOfType<PlayerStart>().transform.rotation;
        }

        public void NextShift()
        {
            _shiftCount++;
            _currentRentPrice = _rentPriceCurve.Evaluate(_shiftCount);

            ReferenceManager.Instance.GetManager<DataManager>().SaveGameData();
            
            ReferenceManager.Instance.GetManager<ItemManager>().SpawnInitialItems(_shiftCount);
            
            OnNextShift?.Invoke();
            
            this.Log(LogType.Log, $"Shift {_shiftCount-1} is over, launching shift {_shiftCount}");
            
            ReferenceManager.Instance.GetManager<UIManager>().ResumeGame();
            
            LaunchShift(_shiftDuration);
        }

        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }
        public void Unregister()
        {
            ReferenceManager.Instance.RemoveManager(this);
        }

        public void Init()
        {
            _dataManager = ReferenceManager.Instance.GetManager<DataManager>();
        }

        public void PostInit()
        {
            //Debug.Log("[ShiftManager] : PostInit running (shift duration set to " + shiftDuration + "s)."); 
            this.Log(LogType.Log, $"PostInit running (shift duration set to {_shiftDuration}s)");
            
            _dataManager.LoadGlobalData();
            //LaunchShift(_shiftDuration); // Removed because it doesn't call everything the right way
            ResetAll();
            NextShift();
        }
        #endregion
    }

}