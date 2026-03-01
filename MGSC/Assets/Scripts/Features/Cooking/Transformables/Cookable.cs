using System;

namespace Features
{
    public class Cookable : CookingFeature
    {
        public float CookingDuration { get; set; } = 3f;
        public bool IsCooked {
            get=> _isCooked;
            set
            {
                if (_isCooked == value) return;
                _isCooked = value;
                if (_isCooked) OnCook?.Invoke();
            }
        }

        public override bool Completed => IsCooked;
        public override bool CanBePerformed => CanBeCooked();

        public Action OnCook;
        public Action OnCookingStart;
        public Action OnCookingEnd;


        #region Fields
        private bool _isCooked;
        #endregion
        #region Methods
        public Cookable(IFeatureCollection owner) : base(owner)
        {
        }

        public void StartCooking()
        {
            OnCookingStart?.Invoke();
        }
        public void StopCooking()
        {
            OnCookingEnd?.Invoke();
        }
        public bool RequestCook() => TryCook();
        private bool TryCook()
        {
            if (!CanBeCooked())
                return false;
            Cook();
            return true;
        }
        private void Cook()
        {
            IsCooked = true;
            StopCooking();
        }
        public bool CanBeCooked()
        {
            if (!Enabled) return false;
            if (IsCooked) return false;

            return true;
        }
        public override void Reset()
        {
            base.Reset();
            IsCooked = false;
        }

        #endregion
    }
}