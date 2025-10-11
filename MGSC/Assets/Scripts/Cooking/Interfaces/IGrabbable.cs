using System;
using UnityEngine;

namespace Cooking
{

    public interface IGrabbable : IGeneric<IGrabbable>
    {
        public IContainer Container { get; set; }
        public Action OnGrabbed { get; set; }
        public void GetGrabbed(IContainer newContainer);
        public Collider Collider { get; }
    }

}