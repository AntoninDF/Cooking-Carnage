

using System;
using static UnityEngine.Rendering.DebugUI;

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


        public Action OnTransformation { get; set; }
        public Action OnBeginTransformation { get; set; }
        public Action OnEndTransformation { get; set; }

        #endregion



        #region Fields

        private bool _transformed;

        #endregion


        #region Methods

        public Transformable(IFeatureCollection owner) : base(owner)
        {
            Conditions = new(this);
        }

        public void BeginTransformation()
        {
            OnBeginTransformation?.Invoke();
        }
        public void EndTransformation()
        {
            OnEndTransformation?.Invoke();
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
            EndTransformation();
        }
        public override void Reset()
        {
            base.Reset();
            Transformed = false;
        }
        #endregion
    }
}