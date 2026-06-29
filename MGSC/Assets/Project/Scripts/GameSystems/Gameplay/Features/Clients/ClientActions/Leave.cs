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
			_client = owner.Owner as Client;
		}

		public override void Perform()
		{
			base.Perform();
			Debug.Log($"[Leave] Perform for {_client.name}");
    
			float   reentryT   = _movement.FindNearestSplineT(_client.transform.position);
			Vector3 reentryPos = _movement.GetSplinePos(reentryT);
    
			// Abonnement AVANT le switch pour ne pas rater le callback
			_movement.OnDestinationReached += () =>
			                                  {
				                                  Debug.Log("[Leave] NavMesh destination reached, switching to spline");
				                                  StartSplineReturn(reentryT);
			                                  };
    
			_movement.SwitchToNavMesh(reentryPos);
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