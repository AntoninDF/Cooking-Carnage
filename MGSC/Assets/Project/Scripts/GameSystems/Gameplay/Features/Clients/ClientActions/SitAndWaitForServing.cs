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
			_seat.TrySit(_client);
			_seat.OnMealReceive        += TryGrabMeal;
			_seat.OnClientUnregister += HandleUnregister;
		}
		
		private void TryGrabMeal()
		{
			if (_seat.Owner is not OrderingArea orderingArea) return;
			var heldItem = orderingArea.ServingArea.MealSlot?.Container?.HeldItem;
			if (heldItem == null) return;
			
			if(orderingArea.ServingArea.MealSlot.Container.RequestDrop(heldItem)) 
				OnMealGrabbed?.Invoke(heldItem);
			if (heldItem.TryGetSibling<F_Recipe>(out var _recipe))
			{
				Debug.LogError("GET THE RECIPE");
			}
			//Reference.GetManager<OrderManager>().ResolveOrder();
			
		}

		/// <summary>
		/// On check si l'item est un plate
		/// si oui : on check l'order/recipe?
		/// le client peu l'evaluer
		/// </summary>
		private void AnalyseHeldItem(F_Grabbable  item)
		{
			
		}

		private void HandleUnregister(Client client)
		{
			if (client != _client) return;
			_seat.OnClientUnregister -= HandleUnregister;
			OnServed?.Invoke();
		}
	}
}