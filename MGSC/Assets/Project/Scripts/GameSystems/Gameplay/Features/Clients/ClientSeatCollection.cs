using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features
{
    public class ClientSeatCollection : Feature
    {
        public List<ClientSeat> Seats { get; private set; } = new();


        public ClientSeatCollection(IFeatureCollection owner) : base(owner)
        {
        }


        public void Register(ClientSeat seat)
        {
            if (seat == null) return;
            if (Seats.Contains(seat)) return;
            Seats.Add(seat);
        }
        public List<ClientSeat> GetClientSeats(bool mustBeAvailable = true) => GetClientSeats(_ => true, mustBeAvailable);
        public List<ClientSeat> GetClientSeats(Func<ClientSeat, bool> filter, bool mustBeAvailable = true)
        {
            List<ClientSeat> result = new();
            foreach (var seat in Seats)
            {
                if (seat == null) continue;
                if (!filter(seat)) continue;
                if (mustBeAvailable && (seat.Enabled == false || seat.RegisteredClient != null)) continue; 

                result.Add(seat);
            }
            return result;
        }
    }

}