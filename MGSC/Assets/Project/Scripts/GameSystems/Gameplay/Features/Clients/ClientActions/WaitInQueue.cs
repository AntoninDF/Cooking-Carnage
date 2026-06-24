namespace Features
{
	
	public class WaitInQueue : BaseClientAction
	{
		
		private float          _targetT;
		private ClientMovement _movement => ClientOwner.ClientMovement;
		
		public WaitInQueue(ClientBehaviour owner) : base(owner) { }

		public override void Perform()
		{
			base.Perform();
			var client = ClientOwner.Owner as Clients.Client;
			var queue  = Reference.GetManager<GameSystem.ClientManager>().WaitingQueue;
			_targetT                   =  queue.Enqueue(client);
			
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
			ClientOwner.OnSplineUpdate -= TickSpline;
		}
    
	}
}