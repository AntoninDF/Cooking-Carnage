using System;

namespace Features
{
	
	public class WaitInQueue : BaseClientAction
	{
		public Action OnEnterQueue { get; set; }
		public Action OnExitQueue { get; set; }
		
		private float          _targetT;
		private ClientMovement _movement => ClientOwner.ClientMovement;
		
		public WaitInQueue(ClientBehaviour owner) : base(owner) { }

		public override void Perform()
		{
			base.Perform();
			var client = ClientOwner.Owner as Clients.Client;
			var queue  = Reference.GetManager<GameSystem.ClientManager>().WaitingQueue;
			_targetT                   =  queue.Enqueue(client);
			
			OnEnterQueue?.Invoke();
			ClientOwner.OnSplineUpdate += TickSpline;
		}
		
		private void TickSpline()
		{
			if (_movement.SplinePosition < _targetT)
				_movement.MoveAlongSpline();
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