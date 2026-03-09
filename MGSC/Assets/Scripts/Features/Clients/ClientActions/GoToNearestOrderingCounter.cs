using System;
using UnityEngine;

namespace Features
{
    public class GoToNearestOrderingCounter : BaseClientAction
    {
        public Action OnCounterReached { get; set; }


        public GoToNearestOrderingCounter(ClientBehaviour owner) : base(owner)
        {
        }

        public override void Perform()
        {
            base.Perform();
            ClientSeat target = GetNearestOrderingCounter();
            if (target == null)
            {
                // No ordering counter found
                Debug.LogWarning("A client tried to go to the nearest ordering counter, but none were found");
                return;
            }
            ClientOwner.ClientMovement.SetDestination(target.transform.position);
            ClientOwner.ClientMovement.OnDestinationReached += OnCounterReached;
        }

        public ClientSeat GetNearestOrderingCounter()
        {
            var seats = ClientManager.GetClientSeats(IsOrderingCounter, mustBeAvailable:true);
            ClientSeat nearestSeat = null;
            float distance = float.MaxValue;
            foreach (var seat in seats)
            {
                float currDist = Vector3.Distance(seat.transform.position, ClientOwner.Agent.transform.position);
                if (currDist < distance)
                {
                    distance = currDist;
                    nearestSeat = seat;
                }
            }
            return nearestSeat;
        }

        public bool IsOrderingCounter(ClientSeat seat)
        {
            if (seat == null) return false;
            if (!seat.TryGetSibling(out OrderingCounter counter, true)) return false;

            return true;
        }
    }
}