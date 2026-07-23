using Features;
using UnityEngine;

namespace Project.Scripts.BloodMachine
{
    public class F_ValveAnimation : Feature
    {
        private Animator _animator;
        public F_ValveAnimation(IFeatureCollection owner, Animator animator) : base(owner)
        {
            _animator = animator;
        }

        public void StartServing()
        {
            _animator.SetTrigger("StartServe");
        }
        
        public void StopServe()
        {
            _animator.SetTrigger("StopServing");
        }

        public void GlassGrabbed()
        {
            _animator.SetTrigger("GlassGrabbed");
        }

        public void ReadyToServe()
        {
            _animator.SetTrigger("ExposeGlass");
        }
        
    }
}