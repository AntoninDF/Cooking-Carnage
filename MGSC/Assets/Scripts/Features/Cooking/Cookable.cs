using System;

namespace Features
{
    public class Cookable : Feature
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
        public Action OnCook;


        #region Fields
        private bool _isCooked;
        #endregion
        #region Methods
        public Cookable(IFeatureCollection owner) : base(owner)
        {
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