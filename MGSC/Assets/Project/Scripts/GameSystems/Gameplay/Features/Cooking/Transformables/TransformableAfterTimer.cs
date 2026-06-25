using System;
using UnityEngine;

namespace Features
{
    public class TransformableAfterTimer : Transformable
    {
        public virtual float TransformationDuration { get; set; } = 5f;
        /// <summary>
        /// OnBeginTransformation is invoked when a transformation isn't instantaneous and is started.
        /// </summary>
        public Action OnBeginTransformation { get; set; }
        public TransformableAfterTimer(IFeatureCollection owner) : base(owner)
        {


            
        }
        public void BeginTransformation()
        {
            OnBeginTransformation?.Invoke();
        }

    }
}