

using System;

namespace Features
{
    public class Transformable : Feature, IFeatureCollection
    {
        #region Properties
        public bool CanBeTransformed => CanBeTransformed_Method();
        public bool Transformed
        {
            get
            {
                return _transformed;
            }
            set
            {
                if (_transformed == value) return;
                _transformed = value;
                if (_transformed) OnTransformation?.Invoke();
            }
        }

        public FeatureCollection Collection { get; set; } = new();
        public Conditions Conditions { get; set; }

        /// <summary>
        /// OnTransformation is invoked immediately every time the transformable is transformed
        /// </summary>
        public Action OnTransformation { get; set; }
        

        #endregion



        #region Fields

        private bool _transformed;

        #endregion


        #region Methods

        public Transformable(IFeatureCollection owner) : base(owner)
        {
            Conditions = new(this);
        }

        public bool RequestTransformation()
        {
            if (!CanBeTransformed_Method())
                return false;
            Transform();
            return true;
        }
        protected virtual bool CanBeTransformed_Method()
        {
            if (!Enabled) return false;
            if (Transformed) return false;
            if (!Conditions.IsValid()) return false;

            return true;
        }
        protected virtual void Transform()
        {
            Transformed = true;
        }
        public override void Reset()
        {
            base.Reset();
            Transformed = false;
        }
        #endregion
    }
}