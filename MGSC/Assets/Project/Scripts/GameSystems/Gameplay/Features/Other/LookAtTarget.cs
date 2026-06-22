using GameSystem;
using System;
using UnityEngine;

namespace Features
{
    public class LookAtTarget : Feature
    {

        public Transform transform { get; set; }
        public Vector3 LookPosition
        {
            get => _lookPosition; set
            {
                _lookPosition = value;
            }
        }
        private Vector3 _lookPosition;
        public bool IsLookedAt
        {
            get => _isLookedAt;
            set
            {
                _lastCheckedFrame = Time.frameCount;
                if (value)
                    RequestResetOnNextFrame();
                if (_isLookedAt == value) return;
                _isLookedAt = value;
                if (_isLookedAt)
                {
                    OnStartLookAt?.Invoke();
                }
                else
                {
                    OnStopLookAt?.Invoke();
                }
            }
        }
        public Action OnStartLookAt { get; set; }
        public Action OnStopLookAt { get; set; }

        #region Fields
        private bool _isLookedAt;
        private TimerManager _timerManager => Reference.GetManager<TimerManager>();
        private int _lastCheckedFrame;
        #endregion

        public LookAtTarget(IFeatureCollection owner, Transform transform) : base(owner)
        {
            this.transform = transform;
        }

        public virtual bool CheckIfLookedAt(Vector3 position, Vector3 direction) => CheckIfLookedAt(position, direction, float.MaxValue);
        public virtual bool CheckIfLookedAt(Vector3 position, Vector3 direction, float distance)
        {
            //UnityEngine.Debug.Log("Look at target checking lookat");
            LookPosition = transform.position;
            float angularTolerance = 30f;

            if (Vector3.Distance(position, transform.position) > distance)
            {
                IsLookedAt = false;
                return false;
            }

            float alignment = Vector3.Dot((transform.position - position).normalized, direction.normalized);

            //UnityEngine.Debug.Log("Alignement is " + alignment);

            if (Mathf.Acos(alignment) / Mathf.PI * 180 < angularTolerance / 2f)
            {
                //UnityEngine.Debug.Log("Angle is " + Mathf.Acos(alignment) / Mathf.PI * 180);
                IsLookedAt = true;
                return true;
            }
            IsLookedAt = false;
            return false;
        }

        public virtual void RequestResetOnNextFrame()
        {
            if (_timerManager != null)
            {
                _timerManager.StartTimer("Reset lookat (" + Time.time + ")", 3, TryReset, true);
            }
        }

        protected virtual void TryReset()
        {
            if (_lastCheckedFrame < Time.frameCount - 1) // Reset only if the last check is at least two frames old
            {
                IsLookedAt = false;
            }
        }
    }
}