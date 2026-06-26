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
        
        
        public Action OnShiftBegin { get; set; }
        public Action OnShiftNoon { get; set; }
        public Action OnShiftEnded { get; set; }
        public Action OnNextShift { get; set; }
        public int ShiftCount => _shiftNumber;
        public float ShiftDuration => _shiftDuration;
        public MoneyInfo CurrentRentPrice => new MoneyInfo(_currentRentPrice);
    
        #endregion

        #region Fields
    
        // Refacto : Ensure all these dependencies are necessary, and won't break the shiftManager if missing
        // Refacto : Ensure that features that depend on other Managers (or even ones that don't) are optional and can be toggled
        private DataManager _dataManager;
        private TimerManager _timerManager;
        private UIManager _uiManager;
        private ItemManager _itemManager;

    
        private int _shiftNumber = -1;
        private float _currentRentPrice = 0 ;
        private AnimationCurve _rentPriceCurve;
        private float _shiftDuration = 120;
        private float _noonDuration = 30;
        
        
    
        #endregion

        #region Methods
   
        public ShiftManager()
        {
            Register();
            Setup();
        }
        // Bind sur globals
        public void Setup()
        {
            Globals.Bind(value => _shiftDuration = value, Globals.Gameplay.ShiftDuration);
            Globals.Bind(value => _noonDuration = value, Globals.Gameplay.ShiftFinalWarningDuration);
            Globals.Bind(value => _rentPriceCurve = value, Globals.Gameplay.ShiftRentPriceCurve);
        }

        public void SetShiftNumber(int number) => _shiftNumber = number;
        public void SetRentPrice(float price) => _currentRentPrice = price;
        public float GetCurrentRentPrice() => GetRentPrice(_shiftNumber);
        public float GetRentPrice(int shiftCount) => _rentPriceCurve.Evaluate(shiftCount);

        public void LaunchShift()=> LaunchShift(_shiftDuration - _noonDuration);
        void LaunchShift(float duration)
        {
            this.Log(LogType.Log, $"Launching new shift for {duration+_noonDuration}s");
            OnShiftBegin?.Invoke();
            _timerManager.StartTimer("shiftDuration", duration, () => LaunchShiftNoon(), false);
            _timerManager.StartTimer("shiftNoonDuration",_noonDuration,() => EndShift(), false);
            _timerManager.StopTimer("shiftNoonDuration");
        }

        void LaunchShiftNoon()
        {
            OnShiftNoon?.Invoke();
            this.Log(LogType.Log, $"{_noonDuration}s left before shift end");
            _timerManager.ResumeTimer("shiftNoonDuration");
        }

        void EndShift()
        {
            OnShiftEnded?.Invoke();
            TryEndShift();
        }

        public void TryEndShift(bool endAsTutorial = false)
        {            
            this.Log(LogType.Log, $"End of the Shift triggered.");

            ResetAll();
            // Normal shift
            if (!endAsTutorial)
            {
                bool validRent = TryPayRent();

                if (validRent)
                    OnShiftSucceeded?.Invoke();
                else
                    OnShiftFailed?.Invoke();
            }
            else
            {
                this.Log("Tutorial has ended, skipping to the next shift...");
                NextShift();
            }
        }


        private bool TryPayRent() => TryPayRent(_shiftNumber);
        private bool TryPayRent(int shiftNumber)
        {
            Reference.GetManager(out MoneyManager moneyManager);
            if (moneyManager == null) return false;

            var rentPrice = GetRentPrice(shiftNumber);
            if (moneyManager.MoneyInfo < rentPrice)
            {
                this.Log(LogType.Log,
                    $"Not enough money ({moneyManager.MoneyInfo}) to pay the rent ({rentPrice}€)");
                moneyManager.AddMoney(-rentPrice);
                return false;
            }
            moneyManager.AddMoney(-rentPrice);
            return true;
        }

        private void ResetAll()
        {
            this.Log("Reset everything for the next shift");

            Reference.GetManager<ItemManager>()?.DestroyAllActive();
            PlayerSetup.TryResetPlayer();
        }

        public void NextShift()
        {
            _shiftNumber++;

            SetupShift(_shiftNumber);

            this.Log(LogType.Log, $"Shift {_shiftNumber - 1} is over, launching shift {_shiftNumber}");

            if (_uiManager) _uiManager.ResumeGame();

            OnNextShift?.Invoke();

            LaunchShift(_shiftDuration - _noonDuration);
        }
        /// <summary>
        /// Resets the specified shift without moving to the next one. 
        /// </summary>
        public void SetupShift(int shiftCount)
        {
            _dataManager.SaveGameData();
            
            ResetAll();
            
            _itemManager.SpawnInitialItems(shiftCount);

        }

        public void Register()
        {
            Reference.SetManager(this);
        }
        public void Unregister()
        {
            Reference.RemoveManager(this);
        }

        public void Init()
        {
            Reference.GetManager(out _dataManager);
            Reference.GetManager(out _timerManager);
            Reference.GetManager(out _itemManager);
        }

        public void PostInit()
        {
            //Debug.Log("[ShiftManager] : PostInit running (shift duration set to " + shiftDuration + "s)."); 
            this.Log(LogType.Log, $"PostInit running (shift duration set to {_shiftDuration}s)");
            
            _dataManager.LoadGlobalData();
            //LaunchShift(_shiftDuration); // Removed because it doesn't call everything the right way
            ResetAll();
            SetupShift(_shiftNumber);
            
        }

        public void SkipTutorialShift()
        {
            if (_shiftNumber == 0)
            {
                _shiftNumber = 1;
                _dataManager.SaveGameData();
            }
        }
        #endregion
    }

}