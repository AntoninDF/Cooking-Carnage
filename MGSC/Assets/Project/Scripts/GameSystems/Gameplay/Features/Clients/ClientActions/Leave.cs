using System;
using Clients;
using UnityEngine;

namespace Features
{
    /// <summary>
    /// This action is performed to make the client re-join the spline and leave the Restaurant
    /// </summary>
    public class Leave : BaseClientAction
    {
        public Action OnExited      { get; set; }

        private ClientMovement _movement;
        private Client         _client;
        
        private float splinePos;
        
        public Leave(ClientBehaviour owner) : base(owner)
        {
            _movement = owner.ClientMovement;
            _client   = owner.Owner as Client;
        }

        public override void Perform()
        {
            base.Perform();
            
            if (_client.transform.parent != null)
            {
                _client.transform.SetParent(null);
                _movement.GetUp();
            }

            splinePos   = _movement.FindNearestSplineT(_client.transform.position);
            Vector3 reentryPos = _movement.GetSplinePos(splinePos);
            
            _movement.OnDestinationReached += OnNavReached;
            _movement.SwitchToNavMesh(reentryPos);
            
        }

       
        void OnNavReached()
        {
            _movement.OnDestinationReached -= OnNavReached; 
            StartSplineReturn(splinePos);
        }

        private void StartSplineReturn(float reentryT)
        {
            _movement.SwitchToSpline(reentryT);
            ClientOwner.OnSplineUpdate += TickSplineExit;
        }

        private void TickSplineExit()
        {
            _movement.MoveAlongSpline();
            if (_movement.SplinePosition >= 1f)
            {
                ClientOwner.OnSplineUpdate -= TickSplineExit;
                OnExited?.Invoke();
            }
        }
    }
}