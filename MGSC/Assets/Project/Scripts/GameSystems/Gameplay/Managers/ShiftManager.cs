using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Upgrades;

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
        public float ShiftDuration => _shiftDuration * Globals.BalancingWeights.ShiftDurationMultiplier;
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
        public void RequestShiftTimer() => LaunchShiftTimer(ShiftDuration);
        void LaunchShiftTimer(float duration)
        {
//            Debug.Log( $"Launching new shift for {duration}s");
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
            AudioSystem.SetEventParameter(AudioSystem.Database.beforeShiftMusic, "Ingame Music Sequence", 2); // 2 is Shift Countdown
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
                // We have to save the game validating the fact that this shift has been done, but the shift number should increase after the shift recap, not before.
                ShiftNumber++;
                SaveManager.SaveGameData();
                ShiftNumber--;

                CheckForAchievements();
            }
            else // If shift is failed
            {
                OnShiftFailed?.Invoke();
                HasFailed = true;
            }


            OnShiftEnded?.Invoke();
            AudioSystem.PlayAudio(AudioSystem.Database.shiftEndSFX);
            GameLoop.RequestTransition(StateTransitions.ShowShiftRecap);
        }

        private void CheckForAchievements()
        {
            if (ShiftNumber == 0) Achievements.Unlock("ACH_A04");
            if (Reference.TryGetManager(out ClientManager cmgr))
            {
                int correct = cmgr.GetCorrectOrdersCount();
                int wrong = cmgr.GetWrongOrdersCount();
                int missed = cmgr.GetMissedOrdersCount();
                int burnt = cmgr.GetBurntOrdersCount();
                int killed = cmgr.ClientsKilledThisShift;
                if (wrong == 0 && correct > 0) Achievements.Unlock("ACH_B01"); // Servir Uniquement de bonnes commandes pendant un service
                if (correct == 0 && wrong > 0) Achievements.Unlock("ACH_B02"); // Servir que des mauvaises commandes pendant un service
                if (correct == 0 && wrong == burnt && burnt > 0) Achievements.Unlock("ACH_B03"); // Servir Uniquement des plats brûlés pendant un service
                if (killed == 0) Achievements.Unlock("ACH_B16"); // Finir un service sans tuer personne (pacifiste)

                var hist = cmgr.GetClientsHistory();
                Debug.Log("Client history : \n" + hist.Select(data => data.Serialize()).ToList().ToString_Detailed());
                // Serve only Eugene
                if (hist.All(data => data.killed || data.timedOut || (data.clientType == Clients.EClientType.A && data.served)) && hist.Any(data => data.served))
                    Achievements.Unlock("ACH_B05");
                // Serve only Mr Fesse
                if (hist.All(data => data.killed || data.timedOut || (data.clientType == Clients.EClientType.B && data.served)) && hist.Any(data => data.served))
                    Achievements.Unlock("ACH_B09");
                // Serve only Fabrice
                if (hist.All(data => data.killed || data.timedOut || (data.clientType == Clients.EClientType.C && data.served)) && hist.Any(data => data.served))
                    Achievements.Unlock("ACH_B07");
                // Kill only Eugene (makes sure all clients killed are Eugene, and there is at list one kill)
                if (hist.All(data => (data.killed && data.clientType == Clients.EClientType.A) || !data.killed) && hist.Any(data => data.killed))
                    Achievements.Unlock("ACH_B06");
                // Kill only Mr. Fesse
                if (hist.All(data => (data.killed && data.clientType == Clients.EClientType.B) || !data.killed) && hist.Any(data => data.killed))
                    Achievements.Unlock("ACH_B10");
                // Kill only Fabrice
                if (hist.All(data => (data.killed && data.clientType == Clients.EClientType.C) || !data.killed) && hist.Any(data => data.killed))
                    Achievements.Unlock("ACH_B08");
                // Serve only recipes containing eyes
                if (hist.All(data => (data.served && data.servedRecipe.Ingredients.All(ingredient => ingredient.name.Contains("Eye"))) || data.killed || data.timedOut) && hist.Any(data => data.served))
                    Achievements.Unlock("ACH_B11");
                // Serve only recipes containing teeth
                if (hist.All(data => (data.served && data.servedRecipe.Ingredients.All(ingredient => ingredient.name.Contains("Tooth"))) || data.killed || data.timedOut) && hist.Any(data => data.served))
                    Achievements.Unlock("ACH_B12");
                // Serve only recipes containing butts
                if (hist.All(data => (data.served && data.servedRecipe.Ingredients.All(ingredient => ingredient.name.Contains("Fiak"))) || data.killed || data.timedOut) && hist.Any(data => data.served))
                    Achievements.Unlock("ACH_B13");
            }
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
        /// <summary> Has to be a positive value </summary>
        public float GetCurrentRentPrice() => GetRentPrice(ShiftNumber);
        /// <summary> Has to be a positive value </summary>
        public float GetRentPrice(int shiftCount)
        {
            float percentage = Globals.BalancingWeights.RentPriceMultiplier;
            if (Reference.TryGetManager(out UpgradeManager um))
            {
                var upgrade = um.GetUpgradeByType<LowerRentUpgrade>();
                if (upgrade != null)
                    percentage *= upgrade.GetRentMultiplier();
            }
            return _rentPriceCurve.Evaluate(shiftCount) * percentage;
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
            this.Log(LogType.Log, $"PostInit running (shift duration set to {ShiftDuration}s)");

            SaveManager.LoadGlobalData();

            //ResetAll();
        }

        public void Init() { }
        #endregion
    }

}