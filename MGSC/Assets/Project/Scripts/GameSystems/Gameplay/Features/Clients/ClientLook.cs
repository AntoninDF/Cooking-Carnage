using System.Collections;
using Clients;
using UnityEngine;

namespace Features
{
    /// <summary>
    /// Handle the look direction of the client 
    /// </summary>
    public class ClientLook : Feature
    {
        
        #region Fields

        private Vector3        _lookDirection     = Vector3.forward;
        private Vector3        _previousDirection = Vector3.forward;
        private Transform      _client;
        private ClientMovement _movement;
        private bool           _isSitting;
        
        private Vector3 _currentDirection;
        private Vector3 _smoothVelocity;
        private float   _smoothTime = 0.15f;

        #endregion

        public ClientLook(IFeatureCollection owner, Transform clientTransform, ClientMovement movement) : base(owner)
        {
            _client   = clientTransform;
            _movement = movement;
        }

        public override void Update()
        {
           
            if (_isSitting)
            {
                return;
            }

            Vector3 targetDirection = GetMovementDirection();
            if (targetDirection == Vector3.zero) return;
            _currentDirection = Vector3.SmoothDamp(_currentDirection, targetDirection, ref _smoothVelocity, _smoothTime);
            if (_currentDirection.sqrMagnitude < 0.001f) return;
            _client.rotation = Quaternion.LookRotation(_currentDirection) * Quaternion.Euler(0, 0, 0);
            _client.rotation = Quaternion.Euler(0, _client.eulerAngles.y, 0);
        }

        private Vector3 GetMovementDirection()
        {
            if (_movement.IsOnSpline)
            {
                float   t       = _movement.SplinePosition;
                Vector3 current = _movement.GetSplinePos(t);
                Vector3 ahead   = _movement.GetSplinePos(Mathf.Clamp01(t + 0.02f));
                Vector3 dir     = (ahead - current);
                return dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.zero;
            }
            else
            {
                Vector3 velocity = _movement.AgentVelocity;
                return velocity.sqrMagnitude > 0.01f ? velocity.normalized : Vector3.zero;
            }
        }

        public void SetClientSitting(Client client)
        {
            _isSitting      = true;
            _smoothVelocity = Vector3.zero;
            (Owner as Client)?.StartCoroutine(SmoothRotationReset());

        }

        public void SetClientLeaving(Client client)
        {
            _isSitting = false;
        }
        

        IEnumerator SmoothRotationReset()
        {
            float      elapsed  = 0;
            float      duration = 0.5f;
            Quaternion startRot = _client.localRotation;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _client.localRotation = Quaternion.Lerp(startRot, Quaternion.Euler(0, 0, 0), elapsed/duration);
                yield return null;
            }
            _client.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }
    
}