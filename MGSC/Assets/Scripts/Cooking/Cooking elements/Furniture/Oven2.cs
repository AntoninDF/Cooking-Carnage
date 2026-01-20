

using GameSystem;
using UnityEngine;

namespace Cooking
{
    public class Oven2 : Oven
    {
        [SerializeField] private GameObject _slider;
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
        }
        protected override void TryCook()
        {
        }

        protected override void StopCooking()
        {
            _isCooking = false;
            OnStopCooking?.Invoke();
            ResetSlider();
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
                _tm.StartTimer(LastTimer, targetDuration / Strength, Cook, onUpdate:(time) => UpdateSlider(time/(targetDuration / Strength)));
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
            ResetSlider();
            _onItemCooked?.Invoke();
            StartOvercooking();
        }

        protected virtual void StartOvercooking()
        {
            var meal = GetHeldItemAs<Meal>();
            var targetRecipe = meal.Recipe;
            if (targetRecipe == null) return;

            _isCooking = true;
            LastTimer = "Overcooking (" + Time.time + ")";
            float duration = targetRecipe.timeToOvercook - targetRecipe.timeToCook; // Add * Strength to account for upgrades on overcooking too
            _tm.StartTimer(LastTimer, duration, Overcook, onUpdate:(time) => UpdateSlider(time/ duration));
        }

        protected void Overcook()
        {
            var meal= GetHeldItemAs<Meal>();
            var targetRecipe = meal.Recipe;
            meal.Cook(targetRecipe.timeToOvercook);
            _onItemOvercooked?.Invoke();
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

        void UpdateSlider(float cookPercentage)
        {
            if (_slider == null) return;
            _slider.transform.localScale = new Vector3(_slider.transform.localScale.x, Mathf.Clamp01(cookPercentage), _slider.transform.localScale.z);
        }

        void ResetSlider()
        {
            if (_slider == null) return;
            _slider.transform.localScale = new Vector3(_slider.transform.localScale.x, 0f, _slider.transform.localScale.z);
        }
        protected T GetHeldItemAs<T>() where T : class => _slot.IsFree ? null : _slot.HeldItem as T;
        protected SO_Recipe GetTargetRecipe() => GetHeldItemAs<Plate>()?.GetMatchingRecipe();
    }
}