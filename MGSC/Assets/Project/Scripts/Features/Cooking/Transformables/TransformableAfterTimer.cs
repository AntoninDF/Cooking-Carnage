using UnityEngine;

namespace Features
{
    public class TransformableAfterTimer : Transformable
    {
        public virtual float TransformationDuration { get; set; } = 5f;
        public TransformableAfterTimer(IFeatureCollection owner) : base(owner)
        {
        }


    }
}