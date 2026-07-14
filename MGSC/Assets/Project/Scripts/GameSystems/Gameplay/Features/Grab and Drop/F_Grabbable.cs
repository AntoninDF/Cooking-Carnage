using System;

namespace Features
{

    public class F_Grabbable : Feature
    {
        public F_Container Container;
        public bool IsLocked
        { 
            get => _isLocked;
            set
            {
                if (_isLocked == value) return;
                _isLocked = value;
                if (value) OnLock?.Invoke();
                else OnUnlock?.Invoke();
            }
        }
        private bool _isLocked = false;
        public Action OnLock { get; set; }
        public Action OnUnlock { get; set; }
        public Action OnGrab {  get; set; }
        
        public Action OnPlayerGrab {  get; set; }

        public F_Grabbable(IFeatureCollection owner) : base(owner)
        {
        }

        public void GetGrabbedBy(F_Container container)
        {
            if (Container == container) return;
            if (IsLocked) return;

            Container?.RequestDrop(this);
            Container = container;

            if (container.Owner is Player)
            {
                OnPlayerGrab?.Invoke();
            }
            OnGrab?.Invoke();
        }
    }

}