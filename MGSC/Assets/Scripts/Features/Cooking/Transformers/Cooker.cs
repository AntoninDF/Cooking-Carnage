using GameSystem;
using System;

namespace Features
{
    public class Cooker : Feature
    {
        public float SpeedMultiplier { get; set; } = 1f;
        /// <summary>
        /// The float parameter tells how far across the cooking is, in percentage (100% is fully cooked).
        /// </summary>
        public Action<float> OnCookingProgress { get; set; }
        public Action<Cookable> OnCookingOver { get; set; }
        public Action<Cookable> OnCookingStart { get; set; }
        private TimerManager TimerManager => Reference.GetManager<TimerManager>();


        #region Methods
        public Cooker(IFeatureCollection owner) : base(owner)
        {
        }
        public bool RequestStartCooking(Cookable cookable)
        {
            if (!CanStartCooking(cookable))
                return false;
            StartCooking(cookable);
            return true;
        }
        public bool RequestCook(Cookable cookable)
        {
            if (!CanCook(cookable))
                return false;
            Cook(cookable);
            return true;
        }

        private void Cook(Cookable cookable)
        {
            cookable.RequestCook();
            OnCookingOver?.Invoke(cookable);
        }

        private void StartCooking(Cookable cookable)
        {
            string timerName = "Cooking (" + UnityEngine.Time.frameCount.ToString("X") + ")";
            var duration = GetCookingDuration(cookable);
            TimerManager.StartTimer(timerName, duration, () => RequestCook(cookable), false, (elapsed) => Progress(elapsed, duration));
            OnCookingStart?.Invoke(cookable);
            cookable.StartCooking();
        }

        // Starting to cook something and actually cooking it is different, hence the separate methods, despite them doing the same thing.
        public bool CanStartCooking(Cookable cookable) => CanCook(cookable);
        public bool CanCook(Cookable cookable)
        {
            if (!cookable.CanBeCooked()) return false;

            return true;
        }
        
        public float GetCookingDuration(Cookable cookable)
        {
            return cookable.CookingDuration * SpeedMultiplier;
        }
      
        private void Progress(float progress, float duration)
        {
            if (duration == 0) duration = 1f;
            OnCookingProgress?.Invoke(progress/duration);
        }

        #endregion
    }
}