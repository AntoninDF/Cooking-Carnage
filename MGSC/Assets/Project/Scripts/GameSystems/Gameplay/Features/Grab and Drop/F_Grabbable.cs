using System;

namespace Features
{

    public class F_Grabbable : Feature
    {
        public F_Container Container;
        public bool IsLocked { get; set; }

        public Action OnGrab {  get; set; }

        public F_Grabbable(IFeatureCollection owner) : base(owner)
        {
        }

        public void GetGrabbedBy(F_Container container)
        {
            if (Container == container) return;
            if (IsLocked) return;

            Container?.RequestDrop(this);
            Container = container;

            OnGrab?.Invoke();
        }
    }

}