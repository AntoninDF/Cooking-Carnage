using System;
using Clients;

namespace Features
{
	public class SitAndWaitForServing : BaseClientAction
	{
		public Action OnServed { get; set; }

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
			if (_seat.Owner is not ServingArea servingArea) return;
			var heldItem = servingArea.MealSlot?.Container?.HeldItem;
			if (heldItem == null) return;
			
			servingArea.MealSlot.Container.RequestDrop(heldItem);
		}

		private void HandleUnregister(Client client)
		{
			if (client != _client) return;
			_seat.OnClientUnregister -= HandleUnregister;
			OnServed?.Invoke();
		}
	}
}