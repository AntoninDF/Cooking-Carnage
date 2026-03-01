using GameSystem;
using System;
using UnityEngine;

namespace Features
{
    public class TransformAfterTimer : Transformer
    {
        public float SpeedMultiplier { get; set; } = 1f;
        /// <summary>
        /// The float parameter tells how far across the cooking is, in percentage (100% is fully cooked).
        /// </summary>
        public Action<float> OnTransformationProgress { get; set; }
        public Action<TransformableAfterTimer> OnTransformationOver { get; set; }
        public Action<TransformableAfterTimer> OnTransformationStart { get; set; }
        private TimerManager TimerManager => Reference.GetManager<TimerManager>();


        #region Methods
        public TransformAfterTimer(IFeatureCollection owner) : base(owner)
        {
        }
        public bool RequestStartTransformation(TransformableAfterTimer target)
        {
            if (!CanStartTransformation(target))
                return false;
            StartTransformation(target);
            return true;
        }

        protected override void Transform(Transformable cookable)
        {
            base.Transform(cookable);
            OnTransformationOver?.Invoke(cookable as Cookable);
        }

        private void StartTransformation(TransformableAfterTimer target)
        {
            string timerName = "Cooking (" + UnityEngine.Time.frameCount.ToString("X") + ")";
            var duration = GetTransformationDuration(target);
            TimerManager.StartTimer(timerName, duration, () => RequestTransformation(target), false, (elapsed) => Progress(elapsed, duration));
            OnTransformationStart?.Invoke(target);
            target.BeginTransformation();
        }

        // Starting to cook something and actually cooking it is different, hence the separate methods, despite them doing the same thing.
        public bool CanStartTransformation(TransformableAfterTimer cookable) => CanTransform(cookable);

        public float GetTransformationDuration(TransformableAfterTimer target)
        {
            return target.TransformationDuration * SpeedMultiplier;
        }

        private void Progress(float progress, float duration)
        {
            if (duration == 0) duration = 1f;
            OnTransformationProgress?.Invoke(progress / duration);
        }

        #endregion
    }
}