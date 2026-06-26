using System;
using Clients;

namespace Features
{
	public class SitAndWaitForServing : BaseClientAction
	{
		public Action OnServed { get; set; }
		public Action OnMealGrabbed { get; set; }

		private ClientSeat _seat;
		private Client     _client;
		
		public SitAndWaitForServing(ClientBehaviour owner, ClientSeat clientSeat) : base(owner)
		{
			_seat   = clientSeat;
			_client = owner.Owner as Client;
		}

		public override void Perform()
		{
			base.Perform();
			_seat.TrySit(_client);
			_seat.OnMealReady        += TryGrabMeal;
			_seat.OnClientUnregister += HandleUnregister;
		}
		
		private void TryGrabMeal()
		{
			if (_seat.Owner is not OrderingArea orderingArea) return;
			var heldItem = orderingArea.ServingArea.MealSlot?.Container?.HeldItem;
			if (heldItem == null) return;
			
			orderingArea.ServingArea.MealSlot.Container.RequestDrop(heldItem);
			OnMealGrabbed?.Invoke();
		}

		private void HandleUnregister(Client client)
		{
			if (client != _client) return;
			_seat.OnClientUnregister -= HandleUnregister;
			OnServed?.Invoke();
		}
	}
}