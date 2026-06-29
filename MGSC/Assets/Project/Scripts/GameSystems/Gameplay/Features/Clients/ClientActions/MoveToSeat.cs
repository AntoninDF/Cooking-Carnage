using System;

namespace Features
{
	
	/// <summary>
	/// this action is performed to make the client move along the spline until the closest point possible of his seat
	/// when the point is reached, the customer switch to navMesh
	/// </summary>
	public class MoveToSeat : BaseClientAction
	{
		public Action<ClientSeat> OnSeatReached { get; set; }

		private ClientSeat     _target;
		private ClientMovement _movement;

		public MoveToSeat(ClientBehaviour owner,
			ClientSeat                    target) : base(owner)
		{
			_target   = target;
			_movement = owner.ClientMovement;
		}

		public override void Perform()
		{
			base.Perform();
			ClientOwner.OnSplineUpdate += CheckSplineProgress;
		}

		/// <summary>
		/// Check si le client arrive au point de la spline le plus proche du seat
		/// </summary>
		private void CheckSplineProgress()
		{
			_movement.MoveAlongSpline();
			
			float nearestT = _movement.FindNearestSplineT(_target.transform.position) - 0.05f; //0.05f est une petite marge pour paraitre naturel

			if (_movement.SplinePosition >= nearestT)
			{
				ClientOwner.OnSplineUpdate -= CheckSplineProgress;
				
				_movement.SwitchToNavMesh(_target.transform.position);
				_movement.OnDestinationReached += OnNavDestinationReached;
			}
		}

		private void OnNavDestinationReached()
		{
			OnSeatReached?.Invoke(_target);
		}
	}
}