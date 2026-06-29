using System;
using GameSystem;
using UnityEngine;

namespace Features
{
    public class Eat : BaseClientAction
    {
        public Action StartEating;
        public Action StopEating;

        public Eat(ClientBehaviour owner, float launchDuration) : base(owner)
        {
            Debug.Log($"[Eat] Created with duration : {launchDuration}");

            StartEating = () =>
            {
                Debug.Log($"[Eat] StartEating, launching timer for {launchDuration}s");

                Reference.GetManager<TimerManager>().StartTimer("Customer Eating Timer", launchDuration, ()=>StopEating?.Invoke(), false);
                
            };
        }

        public override void Perform()
        {
            base.Perform();
            Debug.Log("[Eat] Perform called");
            StartEating?.Invoke();
        }
    }
}