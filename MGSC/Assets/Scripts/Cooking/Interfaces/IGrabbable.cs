using System;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{

    public interface IGrabbable : IGeneric<IGrabbable>
    {
        public IContainer Container { get; set; }
        public UnityEvent OnGrabbed { get; set; }
        public void GetGrabbed(IContainer newContainer);
        public Collider Collider { get; }
    }

}