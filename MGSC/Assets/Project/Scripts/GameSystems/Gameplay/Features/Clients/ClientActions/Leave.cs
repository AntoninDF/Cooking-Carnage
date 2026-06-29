using System;
using Clients;
using UnityEngine;

namespace Features
{
    public class Leave : BaseClientAction
    {
        public Action OnExited { get; set; }

        private ClientMovement _movement;
        private Client         _client;
        
        public Leave(ClientBehaviour owner) : base(owner)
        {
            _movement = owner.ClientMovement;
            _client   = owner.Owner as Client;
        }

        public override void Perform()
        {
            base.Perform();
            Debug.Log($"[Leave] Perform for {_client.name}");

            // 1. S'assurer que le client est bien détaché du siège
            //    (au cas où TryUnregister n'aurait pas encore été appelé)
            if (_client.transform.parent != null)
            {
                _client.transform.SetParent(null);
                _movement.GetUp();
                Debug.Log("[Leave] Detached client from seat parent");
            }

            // 2. Maintenant que la position world est correcte, on cherche le point de spline
            float   reentryT   = _movement.FindNearestSplineT(_client.transform.position);
            Vector3 reentryPos = _movement.GetSplinePos(reentryT);
            
            Debug.Log($"[Leave] reentryT={reentryT}, reentryPos={reentryPos}, clientPos={_client.transform.position}");

            // 3. Abonnement AVANT le switch
            _movement.OnDestinationReached += OnNavReached;
            _movement.SwitchToNavMesh(reentryPos);

            void OnNavReached()
            {
                Debug.Log("[Leave] NavMesh destination reached, switching to spline");
                _movement.OnDestinationReached -= OnNavReached; // sécurité, déjà null-ifié dans CheckNavDestination
                StartSplineReturn(reentryT);
            }
        }

        private void StartSplineReturn(float reentryT)
        {
            Debug.Log($"[Leave] StartSplineReturn at t:{reentryT}");
            _movement.SwitchToSpline(reentryT);
            ClientOwner.OnSplineUpdate += TickSplineExit;
        }

        private void TickSplineExit()
        {
            _movement.MoveAlongSpline();
            Debug.Log($"[Leave] TickSplineExit — SplinePosition: {_movement.SplinePosition}");
            if (_movement.SplinePosition >= 1f)
            {
                ClientOwner.OnSplineUpdate -= TickSplineExit;
                Debug.Log("[Leave] Spline exit complete, invoking OnExited");
                OnExited?.Invoke();
            }
        }
    }
}