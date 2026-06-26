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
			float   reentryT   = _movement.FindNearestSplineT(_client.transform.position);
			Vector3 reentryPos = _movement.GetSplinePos(reentryT);
			
			_movement.SwitchToNavMesh(reentryPos);
			_movement.OnDestinationReached += () => StartSplineReturn(reentryT);
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