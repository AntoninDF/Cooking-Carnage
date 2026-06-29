using System;
using UnityEngine;

namespace Features
{
	/// <summary>
	/// the behavior of the customer in the wait line
	/// </summary>
	public class WaitInQueue : BaseClientAction
	{
		public Action OnEnterQueue { get; set; }
		public Action OnQueuePointReached { get; set; }
		public Action OnExitQueue  { get; set; }
		
		private float          _targetT;
		private bool           _displayed;
		private ClientMovement _movement => ClientOwner.ClientMovement;
		
		public WaitInQueue(ClientBehaviour owner) : base(owner) { }

		public override void Perform()
		{
			base.Perform();
			var client = ClientOwner.Owner as Clients.Client;
			var queue  = Reference.GetManager<GameSystem.ClientManager>().WaitingQueue;
			_targetT            =  queue.Enqueue(client);
			_displayed = false;
			
			OnEnterQueue?.Invoke();
			ClientOwner.OnSplineUpdate += TickSpline;
		}
		
		private void TickSpline()
		{
			if (_movement.SplinePosition < _targetT)
			{
				_movement.MoveAlongSpline();
			}else if (!_displayed)
			{
				OnQueuePointReached?.Invoke();
				_displayed = true;
			}
		}

		public void UpdateTargetT(float newT)
		{
			_targetT = newT;
		}

		public void StopWaiting()
		{
			OnExitQueue?.Invoke();
			ClientOwner.OnSplineUpdate -= TickSpline;
		}
		
    
	}
}