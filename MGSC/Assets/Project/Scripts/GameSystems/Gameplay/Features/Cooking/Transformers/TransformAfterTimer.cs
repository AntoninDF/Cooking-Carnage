using GameSystem;
using System;

namespace Features
{
    public class TransformAfterTimer : Transformer
    {
        public float SpeedMultiplier { get; set; } = 1f;
        /// <summary>
        /// The float parameter tells how far across the cooking is, in percentage betwen 0 and 1 (1 is fully cooked).
        /// </summary>
        public Action<float> OnTransformationProgress { get; set; }
        public Action<TransformableAfterTimer> OnTransformationOver { get; set; }
        public Action<TransformableAfterTimer> OnTransformationStart { get; set; } 
        protected Timer currentProcess;


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
        public bool RequestStopTransformation(TransformableAfterTimer target=null)
        {
            if (currentProcess == null)
                return false;
            currentProcess.Pause();
            currentProcess = null;
            return true;
        }

        protected override void Transform(Transformable cookable)
        {
            base.Transform(cookable);
            OnTransformationOver?.Invoke(cookable as F_Cookable);
        }

        protected virtual void StartTransformation(TransformableAfterTimer target)
        {
            string timerName = "Transforming (" + UnityEngine.Time.frameCount.ToString("X") + ")";
            var duration = GetTransformationDuration(target);
            currentProcess = TimerManager.StartTimer(timerName, duration, () => RequestTransformation(target), (elapsed) => Progress(elapsed, duration));
            GameLoop.BindTimerToGamePause(currentProcess);
            OnTransformationStart?.Invoke(target);
            target.BeginTransformation();
        }

        // Starting to cook something and actually cooking it is different, hence the separate methods, despite them doing the same thing.
        public virtual bool CanStartTransformation(TransformableAfterTimer cookable) => CanTransform(cookable);

        public virtual float GetTransformationDuration(TransformableAfterTimer target)
        {
            return target.TransformationDuration * SpeedMultiplier;
        }

        protected virtual void Progress(float progress, float duration)
        {
            /*
            // Log every second
            if (UnityEngine.Mathf.Round(progress - 0.5f) + UnityEngine.Time.deltaTime > progress)
                UnityEngine.Debug.Log(UnityEngine.Mathf.Round(progress).ToString() + "s elapsed");
            */
            if (duration == 0) duration = 1f;
            OnTransformationProgress?.Invoke(progress / duration);
        }

        #endregion
    }
}