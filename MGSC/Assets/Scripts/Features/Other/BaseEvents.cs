

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

        public override void LateSetup()
        {
            base.LateSetup();
            Start?.Invoke();
        }

        public override void Update()
        {
            base.Update();
            FrameUpdate?.Invoke();
        }
    }

}