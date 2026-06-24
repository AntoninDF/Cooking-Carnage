using System;
using Clients;
using UnityEngine;

namespace Features
{
	public class SelectSeat : BaseClientAction
	{
		public Action<ClientSeat> OnSeatFound    { get; set; }
		public Action             OnSeatNotFound { get; set; }
		
		public SelectSeat(ClientBehaviour owner) : base(owner) { }
		
		/// <summary>
		/// Found the nearest seat among the available
		/// </summary>
		public override void Perform()
		{
			base.Perform();
			var seats = ClientManager.GetClientSeats(mustBeAvailable: true);
			if (seats.Count == 0)
			{
				OnSeatNotFound?.Invoke();
				return;
			}

			ClientSeat nearestSeat = null;
			float      minDist = float.MaxValue;

			foreach (var seat in seats)
			{
				float dist = Vector3.Distance(seat.transform.position, ClientOwner.Agent.transform.position);
				if (dist < minDist)
				{
					minDist = dist;
					nearestSeat = seat;
				}
			}

			if (nearestSeat != null)
			{
				nearestSeat.TryRegister(ClientOwner.Owner as Client);
			}
			else
			{
				Debug.LogWarning("No nearest Seat");
			}
			 
			OnSeatFound?.Invoke(nearestSeat);
		}
	}
}