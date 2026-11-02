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
			//_seat.OnMealReady -= TryGrabMeal;  j'ai cpùùenté ceci mais ça cause probablement des bugs, we'll see

			if (_seat.Owner is not OrderingArea orderingArea)
				return;

			var servingSlotContainer  = orderingArea.ServingArea.ServingSlot?.Container;
			var drinkSlotContainer = orderingArea.ServingArea.DrinkSlot?.Container;
			var mealSlotContainer = orderingArea.ServingArea.MealSlot?.Container;

			var heldItem = servingSlotContainer?.HeldItem;

			
			if (heldItem == null || !heldItem.TryGetSibling(out Servable servable, true)) return;

			servingSlotContainer.IsLocked = false;
			servingSlotContainer.RequestDrop(heldItem);
			
			if (servable.DrinkClientType != 0)
				drinkSlotContainer.RequestGrab(heldItem);

			else
			{
				mealSlotContainer.RequestGrab(heldItem);
                
				//servingSlotContainer.IsLocked = true;
			}
			
			heldItem.IsLocked          = true;
			
			_client.TryAcceptServing(servable, servable.DrinkClientType);

			if (heldItem.Owner is Component itemObject)
			{
				_client.Behaviour.OnEatEnd += () =>
				{
					heldItem.IsLocked          = false;
					heldItem?.Container?.RequestDrop(heldItem);
                    UnityEngine.Object.Destroy(itemObject.gameObject);
				};
				_client.Cooked.OnCustomerCooked += () =>
				{
					heldItem.IsLocked          = false;
					heldItem?.Container?.RequestDrop(heldItem);
					UnityEngine.Object.Destroy(itemObject.gameObject);
					
					/*
					var ds = orderingArea.ServingArea.DrinkSlot;
					if (ds?.HeldItem != null) ds.HeldItem.IsLocked = false;
					ds.Container.Empty();
					var ms = orderingArea.ServingArea.MealSlot;
					if (ms?.HeldItem != null) ms.HeldItem.IsLocked = false;
					ms.Container.Empty();
					UnityEngine.Object.Destroy(itemObject.gameObject);*/
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