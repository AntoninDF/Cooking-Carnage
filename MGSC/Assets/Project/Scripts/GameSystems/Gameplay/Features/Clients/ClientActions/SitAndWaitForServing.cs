using System;
using Clients;
using UnityEngine;

namespace Features
{
	public class SitAndWaitForServing : BaseClientAction
	{
		public Action OnServed { get; set; }
		public Action<F_Grabbable> OnMealGrabbed { get; set; }

		private ClientSeat _seat;
		private Client     _client;
		private F_Grabbable _grabbedObject;
		
		public SitAndWaitForServing(ClientBehaviour owner, ClientSeat clientSeat) : base(owner)
		{
			_seat   = clientSeat;
			_client = owner.Owner as Client;
		}

		public override void Perform()
		{
			base.Perform();
			bool sat = _seat.TrySit(_client);
			_seat.OnMealReady			+= _client.Behaviour.OnMealServed;
			_seat.OnMealReady			+= TryGrabMeal;
			_seat.OnClientUnregister	+= HandleUnregister;

		}
		
		private void TryGrabMeal()
		{
			_seat.OnMealReady -= TryGrabMeal;

			if (_seat.Owner is not OrderingArea orderingArea)
				return;

			var mealSlotContainer  = orderingArea.ServingArea.ServingSlot?.Container;
			//var drinkSlotContainer = orderingArea.ServingArea.SecondarySlot?.Container;

			var heldItem = mealSlotContainer?.HeldItem;
			if (heldItem == null) return;
			if (!heldItem.TryGetSibling(out Servable servable, true)) return;

			//var          heldDrink       = drinkSlotContainer?.HeldItem;
			GameObject   drinkObject     = null;
			EClientType? servedDrinkType = null;

			/*
			if (heldDrink != null && heldDrink.Owner is Drink drinkComp)
			{
				drinkObject     = (drinkComp as Component)?.gameObject;
				servedDrinkType = orderingArea.ServingArea.ClientSeat.SittingClient.Type;
			}
			*/

			mealSlotContainer.IsLocked = false;
			mealSlotContainer.RequestDrop(heldItem);
			heldItem.IsLocked          = true;
			mealSlotContainer.IsLocked = true;

			/*
			if (heldDrink != null && drinkSlotContainer != null)
			{
				drinkSlotContainer.IsLocked = false;
				drinkSlotContainer.RequestDrop(heldDrink);
				heldDrink.IsLocked          = true;
				drinkSlotContainer.IsLocked = true;
			}
			*/

			_client.TryAcceptServing(servable, servedDrinkType);

			if (heldItem.Owner is Component plateComp)
			{
				_client.Behaviour.OnEatEnd += () =>
				                              {
					                              UnityEngine.Object.Destroy(plateComp.gameObject);
					                              if (drinkObject != null) UnityEngine.Object.Destroy(drinkObject);
				                              };
			}
		}
		

		private void HandleUnregister(Client client)
		{
			_seat.OnMealReady        -= TryGrabMeal; 
			if (client != _client) return;
			_seat.OnClientUnregister -= HandleUnregister;
			OnServed?.Invoke();
		}
	}
}