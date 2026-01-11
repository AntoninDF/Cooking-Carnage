

using GameSystem;
using UnityEngine;

namespace Cooking
{
    public class Oven2 : Oven
    {
        private string LastTimer { get => _timerID; set { _tm.RemoveTimer(_timerID); _timerID = value; } }

        TimerManager _tm;
        string _timerID;
        protected override void Start()
        {
            ReferenceManager.Get(out _tm);
            _slot.OnDrop.AddListener((item) => StopCooking());
        }
        protected override void Update()
        {
            Debug.Log("HeldItem is : " + _slot.HeldItem + "(Time : " + Time.time + ")");
        }
        protected override void TryCook()
        {
        }

        protected override void StopCooking()
        {
            _isCooking = false;
            OnStopCooking?.Invoke();
            _tm.StopTimer(_timerID);
        }
        public override void StartCooking()
        {
            var plate = GetHeldItemAs<Plate>();

            // If it's a plate, cook
            if (plate != null)
            {
                var targetRecipe = GetTargetRecipe();
                if (targetRecipe == null) return;

                float targetDuration = targetRecipe.timeToCook;

                _isCooking = true;
                OnStartCooking?.Invoke();
                LastTimer = "Cooking (" + Time.time + ")";
                _tm.StartTimer(LastTimer, targetDuration * Strength, Cook);
            }
            // Otherwise, it may be a cooked meal, so try to overcook it.
            else if (GetHeldItemAs<Meal>() != null)
            {
                StartOvercooking();
                OnStartCooking?.Invoke();
            }
        }

        protected override void Cook()
        {
            var plate = GetHeldItemAs<Plate>();
            var targetRecipe = GetTargetRecipe();
            plate.Cook(targetRecipe.timeToCook + 0.01f);
            StartOvercooking();
        }

        protected virtual void StartOvercooking()
        {
            var meal = GetHeldItemAs<Meal>();
            var targetRecipe = meal.Recipe;
            if (targetRecipe == null) return;

            float targetDuration = targetRecipe.timeToOvercook;

            _isCooking = true;
            LastTimer = "Overcooking (" + Time.time + ")";
            _tm.StartTimer(LastTimer, targetRecipe.timeToOvercook, Overcook);
        }

        protected void Overcook()
        {
            var meal= GetHeldItemAs<Meal>();
            var targetRecipe = meal.Recipe;
            meal.Cook(targetRecipe.timeToOvercook);
        }

        public override void ToggleCooking()
        {
            if (_isCooking)
            {
                StopCooking();
                return;
            }
            if (_slot.IsFree)
            {
                OnFailedStartAttempt?.Invoke();
                return;
            }
            StartCooking();
        }

        protected T GetHeldItemAs<T>() where T : class => _slot.IsFree ? null : _slot.HeldItem as T;
        protected SO_Recipe GetTargetRecipe() => GetHeldItemAs<Plate>()?.GetMatchingRecipe();
    }
}