using GameSystem;
using System;
using UnityEngine;

namespace Features
{
    public class F_LookAtTarget : Feature
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
        #endregion

        public F_LookAtTarget(IFeatureCollection owner, Transform transform) : base(owner)
        {
            this.transform = transform;
        }

        public virtual bool CheckIfLookedAt(Vector3 position, Vector3 direction) => CheckIfLookedAt(position, direction, float.MaxValue);
        public virtual bool CheckIfLookedAt(Vector3 position, Vector3 direction, float distance)
        {
            if (transform == null) 
            {
                IsLookedAt = false;
                return false;
            }
            
            LookPosition = transform.position;

            if (transform.TryGetComponent(out Collider col))
            {
                if (Physics.Raycast(position, direction, out RaycastHit hit, distance))
                {
                    LookPosition = hit.point;
                    IsLookedAt = true;
                    return true;
                }
                else
                {
                    IsLookedAt = false;
                    return false;
                }
            }


            float angularTolerance = 30f;

            if (Vector3.Distance(position, transform.position) > distance)
            {
                IsLookedAt = false;
                return false;
            }

            float alignment = Vector3.Dot((transform.position - position).normalized, direction.normalized);

            if (Mathf.Acos(alignment) / Mathf.PI * 180 < angularTolerance / 2f)
            {
                IsLookedAt = true;
                return true;
            }
            IsLookedAt = false;
            return false;
        }

    }
}