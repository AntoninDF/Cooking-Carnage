using Controller;
using System;
using UnityEngine;

namespace GameSystem
{
    public class ShiftManager : IManager
    {
        #region Properties
        [GlobalEvent("Shift/OnBegin")]
        public static Action OnShiftBegin { get; set; }
        [GlobalEvent("Shift/OnTimerStart")]
        public static Action OnShiftTimerStart { get; set; }
        [GlobalEvent("Shift/OnFinalWarning")]
        public static Action OnShiftFinalWarning { get; set; }
        [GlobalEvent("Shift/OnFail")]
        public static Action OnShiftFailed { get; set; }
        [GlobalEvent("Shift/OnSuccess")]
        public static Action OnShiftSucceeded { get; set; }
        [GlobalEvent("Shift/OnEnd")]
        public static Action OnShiftEnded { get; set; }
        [GlobalEvent("Shift/OnNextShiftTriggered")]
        public static Action OnNextShift { get; set; }

        public int ShiftNumber { get => Globals.Gameplay.ShiftNumber; set => Globals.Gameplay.ShiftNumber.Value = value; }
        public float ShiftDuration => _shiftDuration;
        public bool IsShiftActive { get; set; }
        /// <summary>
        /// Can be null if the timer hasn't been launched yet
        /// </summary>
        public Timer TimerUntilEndOfShift { get; private set; }
        /// <summary>
        /// Can be null if the timer hasn't been launched yet
        /// </summary>
        public Timer TimerUntilFinalWarning { get; private set; }

        #endregion

        #region Fields

        // Refacto : Ensure all these dependencies are necessary, and won't break the shiftManager if missing
        // Refacto : Ensure that features that depend on other Managers (or even ones that don't) are optional and can be toggled

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
        public float GetCurrentRentPrice() => GetRentPrice(ShiftNumber);
        public float GetRentPrice(int shiftCount) => _rentPriceCurve.Evaluate(shiftCount);

        /// <summary>
        /// Simply invokes <see cref="OnShiftBegin"/>, and starts the timers for the end of shift.
        /// </summary>
        public void LaunchShiftTimer() => LaunchShiftTimer(_shiftDuration);
        void LaunchShiftTimer(float duration)
        {
            this.Log(LogType.Log, $"Launching new shift for {duration}s");
            IsShiftActive = true;
            OnShiftBegin?.Invoke();
            TimerUntilEndOfShift = TimerManager.StartTimer("shiftDuration", duration, EndShift, false);
            TimerUntilFinalWarning = TimerManager.StartTimer("shiftDurationWithoutNoon", duration - _noonDuration, InvokeFinalWarning, false);
            // Subscribe to the GameLoop in case the game pauses
            GameLoop.BindTimer(TimerUntilEndOfShift);
            GameLoop.BindTimer(TimerUntilFinalWarning);
        }

        void InvokeFinalWarning()
        {
            OnShiftFinalWarning?.Invoke();
            if (TimerUntilFinalWarning != null)
            {
                GameLoop.UnbindTimer(TimerUntilFinalWarning);
                TimerUntilFinalWarning = null;
            }
            this.Log(LogType.Log, $"{_noonDuration}s left before shift end");
        }

        /// <summary>
        /// Invokes <see cref="OnShiftEnded"/>,
        /// tries to pay the rent and invokes <see cref="OnShiftSucceeded"/> or 
        /// <see cref="OnShiftFailed"/> based on wether the rent could be payed or not.
        /// </summary>
        void EndShift()
        {
            this.Log(LogType.Log, $"End of the Shift triggered.");
            IsShiftActive = false;
            OnShiftEnded?.Invoke();
            if (TimerUntilEndOfShift != null)
            {
                GameLoop.UnbindTimer(TimerUntilEndOfShift);
                TimerUntilEndOfShift = null;
            }

            bool validRent = TryPayRent();

            if (validRent)
                OnShiftSucceeded?.Invoke();
            else
                OnShiftFailed?.Invoke();

            SaveManager.SaveGameData();
        }

        /// <summary>
        /// Tries to pay the rent due for the current shift (<see cref="ShiftNumber"/>)
        /// </summary>
        /// <returns>Wether or not the rent could be payed. False can either mean there was no money manager, or the money owned wasn't sufficient. True means the money has been payed</returns>
        private bool TryPayRent()
        {
            Reference.GetManager(out MoneyManager moneyManager);
            if (moneyManager == null) return false;

            var rentPrice = GetRentPrice(ShiftNumber);
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

        /// <summary>
        /// Resets all the items and the player
        /// </summary>
        private void ResetAll()
        {
            Debug.Log("Reset everything for the next shift");

            Reference.GetManager<ItemManager>()?.DestroyAllActive();
            PlayerSetup.TryResetPlayer();
        }

        public void NextShift()
        {
            Debug.Log("NextShift called");
            ShiftNumber++;

            SetupShift(ShiftNumber);

            this.Log(LogType.Log, $"Shift {ShiftNumber - 1} is over, launching shift {ShiftNumber}");

            OnNextShift?.Invoke();
        }
        /// <summary>
        /// Resets the specified shift without moving to the next one. 
        /// </summary>
        public void SetupShift(int shiftCount)
        {
            SaveManager.SaveGameData();
            
            ResetAll();

            Reference.GetManager<ItemManager>()?.SpawnInitialItems(shiftCount);

        }

        public void Register()
        {
            Reference.SetManager(this);
        }
        public void Unregister()
        {
            Reference.RemoveManager(this);
        }


        public void PostInit()
        {
            this.Log(LogType.Log, $"PostInit running (shift duration set to {_shiftDuration}s)");

            SaveManager.LoadGlobalData();

            SetupShift(ShiftNumber);
        }

        public void SkipTutorialShift()
        {
            if (ShiftNumber == 0)
            {
                ShiftNumber = 1;
                SaveManager.SaveGameData();
            }
        }

        public void Init() { }
        #endregion
    }

}