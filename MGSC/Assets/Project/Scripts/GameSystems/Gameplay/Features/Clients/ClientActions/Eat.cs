using System;
using GameSystem;
using UnityEngine;

namespace Features
{
    /// <summary>
    /// Action performed to make the client eat and use a timer for the animation Time
    /// </summary>
    public class Eat : BaseClientAction
    {
        public Action StartEating;
        public Action StopEating;

        public Eat(ClientBehaviour owner, float launchDuration) : base(owner)
        {
            StartEating = () =>
            {
                TimerManager.StartTimer("Customer Eating Timer", launchDuration, ()=>StopEating?.Invoke(), false);
            };
        }

        public override void Perform()
        {
            base.Perform();
            StartEating?.Invoke();
        }
    }
}