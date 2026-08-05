using Features;
using UnityEngine;

namespace Project.Scripts.BloodMachine
{
    public class F_ValveAnimation : Feature
    {
        private Animator      _animator;
        private AnimationClip _startServeClip;
        private float         _startServeDuration;

        public F_ValveAnimation(IFeatureCollection owner, Animator animator, AnimationClip startServeClip, float startServeDuration) : base(owner)
        {
            _animator           = animator;
            _startServeClip     = startServeClip;
            _startServeDuration = startServeDuration;
        }

        public void StartServing()
        {
            Debug.Log(_startServeClip.name);
            if (_startServeClip != null && _startServeDuration > 0f)
                _animator.speed = _startServeClip.length / _startServeDuration;
            else
                _animator.speed = 1f;

            _animator.SetTrigger("StartServe");
        }
        
        public void ResetSpeed() => _animator.speed = 1f;

        public void StopServe()
        {
            ResetSpeed();
            _animator.SetTrigger("StopServe");
        }

        // public void GlassGrabbed() => _animator.SetTrigger("GlassGrabbed");
        // public void ReadyToServe() => _animator.SetTrigger("ExposeGlass");
    }
}