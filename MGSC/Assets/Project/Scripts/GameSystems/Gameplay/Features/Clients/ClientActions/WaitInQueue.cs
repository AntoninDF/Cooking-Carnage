namespace Features
{
	
	public class WaitInQueue : BaseClientAction
	{
		public WaitInQueue(ClientBehaviour owner) : base(owner) { }

		public override void Perform()
		{
			base.Perform();
			ClientOwner.OnSplineUpdate += TickSpline;
		}
		
		private void TickSpline()
		{
			_movement.MoveAlongSpline();
		}
    
		public void StopWaiting()
		{
			ClientOwner.OnSplineUpdate -= TickSpline;
		}
    
		private ClientMovement _movement => ClientOwner.ClientMovement;
	}
}