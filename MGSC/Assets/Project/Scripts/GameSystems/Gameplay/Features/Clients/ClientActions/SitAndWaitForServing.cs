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
			Debug.Log($"[SitAndWait] TrySit result : {sat} for {_client.name}");
			_seat.OnMealReady        += TryGrabMeal;
			_seat.OnClientUnregister += HandleUnregister;
			Debug.Log($"[SitAndWait] {_client.name} is now sitting and waiting for meal");

		}
		
		private void TryGrabMeal()
		{
			_seat.OnMealReady -= TryGrabMeal;
    
			if (_seat.Owner is not OrderingArea orderingArea)
			{
				Debug.LogWarning("[SitAndWait] Seat owner is not ServingArea");
				return;
			}
			var heldItem = orderingArea.ServingArea.MealSlot?.Container?.HeldItem;
			if (heldItem == null)
			{
				Debug.LogWarning("[SitAndWait] No item in MealSlot");
				return;
			}
			if (!heldItem.TryGetSibling(out Servable servable, true))
			{
				Debug.LogWarning("[SitAndWait] HeldItem has no Servable sibling — Servable pas dans la collection du Plate");
				return;
			}

			Debug.Log("[SitAndWait] Servable found, dropping and serving");
			orderingArea.ServingArea.MealSlot.Container.RequestDrop(heldItem);
			_client.TryAcceptServing(servable);
			
			if (heldItem.Owner is Component plateComp)
			{
				if (heldItem.TryGetSibling(out F_GoToAnimation anim))
					anim.Enabled = false;

				Debug.Log($"[SitAndWait] Destroying served plate : {plateComp.gameObject.name}");
				UnityEngine.Object.Destroy(plateComp.gameObject);
			}
			
		}
		

		private void HandleUnregister(Client client)
		{
			_seat.OnMealReady        -= TryGrabMeal; 
			Debug.Log($"[SitAndWait] HandleUnregister called for {client.name}, expected {_client.name}");
			if (client != _client) return;
			Debug.Log($"[SitAndWait] {_client.name} unregistered, invoking OnServed");
			_seat.OnClientUnregister -= HandleUnregister;
			OnServed?.Invoke();
		}
	}
}