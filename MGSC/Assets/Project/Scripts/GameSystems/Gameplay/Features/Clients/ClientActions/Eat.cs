using System;
using GameSystem;

namespace Features
{
    public class Eat : BaseClientAction
    {
        public Action StartEating;
        public Action StopEating;

        public Eat(ClientBehaviour owner, float launchDuration) : base(owner)
        {
            StartEating = () =>
            {
                Reference.GetManager<TimerManager>()
                    .StartTimer("Customer Eating Timer", launchDuration, ()=>StopEating?.Invoke(), true);
                
            };
        }

        public override void Perform()
        {
            base.Perform();
            StartEating?.Invoke();
        }
    }
}