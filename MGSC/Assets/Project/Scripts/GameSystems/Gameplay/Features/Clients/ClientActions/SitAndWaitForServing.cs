using System;
using Clients;
using Cooking;
using GameSystem;
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

			var heldItem = orderingArea.ServingArea.MealSlot?.Container?.HeldItem;
			if (heldItem == null)
				return;
			if (!heldItem.TryGetSibling(out Servable servable, true))
				return;

			orderingArea.ServingArea.MealSlot.Container.IsLocked = false;
			orderingArea.ServingArea.MealSlot.Container.RequestDrop(heldItem);
			heldItem.IsLocked = true;
			orderingArea.ServingArea.MealSlot.Container.IsLocked = true;
			
			_client.TryAcceptServing(servable);

			if (heldItem.Owner is Component plateComp)
			{
				if (heldItem.TryGetSibling(out F_GoToAnimation anim) && anim.CanAnimate())
				{
					anim.OnAnimationStop += () =>
					                        {
						                        anim.Enabled = false;
						                        _client.Behaviour.OnEatEnd += () => UnityEngine.Object.Destroy(plateComp.gameObject);
					                        };
				}
				else
				{
					_client.Behaviour.OnEatEnd += () => UnityEngine.Object.Destroy(plateComp.gameObject);
					
				}
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