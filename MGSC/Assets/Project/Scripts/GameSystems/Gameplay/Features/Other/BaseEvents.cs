

using System;

namespace Features
{
    public class BaseEvents : Feature
    {
        public BaseEvents(IFeatureCollection owner) : base(owner)
        {
        }
        public Action Start { get; set; }
        public Action FrameUpdate { get; set; }
        public Action CustomEvent0 { get; set; }
        public Action CustomEvent1 { get; set; }
        public Action CustomEvent2 { get; set; }
        public Action CustomEvent3 { get; set; }

        public override void LateSetup()
        {
            base.LateSetup();
            Start?.Invoke();
        }

        public override void Update()
        {
            FrameUpdate?.Invoke();
        }
        public void InvokeCustomEvent0() => CustomEvent0?.Invoke();
        public void InvokeCustomEvent1() => CustomEvent1?.Invoke();
        public void InvokeCustomEvent2() => CustomEvent2?.Invoke();
        public void InvokeCustomEvent3() => CustomEvent3?.Invoke();
    }

}