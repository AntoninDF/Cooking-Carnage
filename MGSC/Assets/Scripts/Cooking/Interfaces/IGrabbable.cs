using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{

    public interface IGrabbable : IGeneric<IGrabbable>
    {
        public IContainer Container { get; set; }
        public UnityEvent OnGrabbed { get;}
        public void GetGrabbedBy(IContainer newContainer);
        public void SetAnimationMode(bool animate=true);
        public Collider Collider { get; }
        public bool IsLocked { get; set; }
    }

}