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
        [GlobalEvent("Shift/OnEnd")]
        public static Action OnShiftEnded { get; set; }
        [GlobalEvent("Shift/OnFail")]
        public static Action OnShiftFailed { get; set; }
        [GlobalEvent("Shift/OnSuccess")]
        public static Action OnShiftSucceeded { get; set; }
        [GlobalEvent("Shift/OnNextShiftTriggered")]
        public static Action OnNextShift { get; set; }

        public int ShiftNumber { get => Globals.Gameplay.ShiftNumber; set => Globals.Gameplay.ShiftNumber.Value = value; }
        public float ShiftDuration => _shiftDuration;
        public bool IsShiftActive { get; set; }
        public bool HasFailed { get; set; }
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
            GameLoop.OnReset += Reset;
        }

        void Reset()
        {
            HasFailed = false;
            IsShiftActive = false;
            ResetTimerUntilEndOfShift();
            ResetTimerUntilFinalWarning();
            if (GameLoop.CurrentState is ShiftState)
                GameLoop.SetState<PreShiftState>();
        }
        // Bind sur globals
        public void Setup()
        {
            Globals.Bind(value => _shiftDuration = value, Globals.Gameplay.ShiftDuration);
            Globals.Bind(value => _noonDuration = value, Globals.Gameplay.ShiftFinalWarningDuration);
            Globals.Bind(value => _rentPriceCurve = value, Globals.Gameplay.ShiftRentPriceCurve);
        }

        void BeginShift()
        {
            if (IsShiftActive) return;
            // Eventually check if the request should be granted or not
            IsShiftActive = true;
            OnShiftBegin?.Invoke();
        }

        /// <summary>
        /// Simply invokes <see cref="OnShiftBegin"/>, and starts the timers for the end of shift.
        /// </summary>
        public void RequestShiftTimer() => LaunchShiftTimer(_shiftDuration);
        void LaunchShiftTimer(float duration)
        {
            TimerUntilEndOfShift = TimerManager.StartGameplayTimer("shiftDuration", duration, EndShift);
            TimerUntilFinalWarning = TimerManager.StartGameplayTimer("shiftDurationWithoutNoon", duration - _noonDuration, InvokeFinalWarning);
            // AddListener to the GameLoop in case the game pauses
            OnShiftTimerStart?.Invoke();
            BeginShift();
        }

        void InvokeFinalWarning()
        {
            ResetTimerUntilFinalWarning();
            this.Log(LogType.Log, $"{_noonDuration}s left before shift end");
            OnShiftFinalWarning?.Invoke();
            AudioSystem.PlayAudio(AudioSystem.Database.shiftNoonSFX);
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
            ResetTimerUntilEndOfShift();

            bool validRent = TryPayRent();

            if (validRent)
            {
                OnShiftSucceeded?.Invoke();
            }
            else
            {
                OnShiftFailed?.Invoke();
                HasFailed = true;
            }

            // We have to save the game validating the fact that this shift has been done, but the shift number should increase after the shift recap, not before.
            ShiftNumber++;
            SaveManager.SaveGameData();
            ShiftNumber--;
            OnShiftEnded?.Invoke();
            AudioSystem.PlayAudio(AudioSystem.Database.shiftEndSFX);
            GameLoop.RequestTransition(StateTransitions.ShowShiftRecap);
        }

        private void ResetTimerUntilEndOfShift()
        {
            if (TimerUntilEndOfShift != null)
            {
                GameLoop.UnbindTimerFromGamePause(TimerUntilEndOfShift);
                TimerUntilEndOfShift.Reset();
                TimerManager.RemoveTimer(TimerUntilEndOfShift.Name);
                TimerUntilEndOfShift = null;
            }
        }

        private void ResetTimerUntilFinalWarning()
        {
            if (TimerUntilFinalWarning != null)
            {
                GameLoop.UnbindTimerFromGamePause(TimerUntilFinalWarning);
                TimerUntilFinalWarning.Reset();
                TimerManager.RemoveTimer(TimerUntilFinalWarning.Name);
                TimerUntilFinalWarning = null;
            }
        }

        bool TryPayRent()
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
        public float GetCurrentRentPrice() => GetRentPrice(ShiftNumber);
        public float GetRentPrice(int shiftCount) => _rentPriceCurve.Evaluate(shiftCount);

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

            //ResetAll();
        }

        public void Init() { }
        #endregion
    }

}