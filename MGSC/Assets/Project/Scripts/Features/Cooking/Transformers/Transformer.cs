using System;
namespace Features
{
    /// <summary>
    /// Directed by Michael Bay.
    /// </summary>
    public class Transformer : Feature, IFeatureCollection
    {
        #region Properties

        public Action OnTransformation { get; set; }

        public Conditions Conditions { get; set; }

        public FeatureCollection Collection { get; set; } = new();

        #endregion
        #region Methods

        public Transformer(IFeatureCollection owner) : base(owner)
        {
            Conditions = new(this);
        } 

        public bool RequestTransformation(Transformable target)
        {
            if (!CanTransform(target))
                return false;
            Transform(target);
            return true;
        }

        public virtual bool CanTransform(Transformable target)
        {
            if (!Enabled) return false;
            if (target == null) return false;
            if (!target.CanBeTransformed) return false;
            if (!Conditions.IsValid(target)) return false;

            return true;
        }

        protected virtual void Transform(Transformable target)
        {
            target.RequestTransformation();
            OnTransformation?.Invoke();
        }

        #endregion
    }
}