// using System;
// using UnityEngine;
//
// namespace Features
// {
//     public class GoToNearestClientSeat : BaseClientAction, IFeatureCollection
//     {
//         public Action<ClientSeat> OnClientSeatReached { get; set; }
//
//         public ClientSeat NearestCounter => GetNearestClientSeat();
//
//         public Conditions Filter { get; set; }
//
//         public FeatureCollection Collection { get; set; } = new();
//
//         public GoToNearestClientSeat(ClientBehaviour owner) : base(owner)
//         {
//             Filter = new(this);
//         }
//
//         public override void Perform()
//         {
//             base.Perform();
//             ClientSeat target = GetNearestClientSeat();
//             if (target == null)
//             {
//                 // No ordering counter found
//                 Debug.LogWarning("A client tried to go to the nearest client seat, but none were found");
//                 return;
//             }
//             if (!Filter.IsValid(target)) return;
//             ClientOwner.ClientMovement.SetDestination(target.transform.position);
//             ClientOwner.ClientMovement.OnDestinationReached += () => OnClientSeatReached?.Invoke(target);
//         }
//
//         public ClientSeat GetNearestClientSeat()
//         {
//             var seats = ClientManager.GetClientSeats((seat) => Filter.IsValid(seat), mustBeAvailable:true);
//             ClientSeat nearestSeat = null;
//             float distance = float.MaxValue;
//             foreach (var seat in seats)
//             {
//                 float currDist = Vector3.Distance(seat.transform.position, ClientOwner.Agent.transform.position);
//                 if (currDist < distance)
//                 {
//                     distance = currDist;
//                     nearestSeat = seat;
//                 }
//             }
//             return nearestSeat;
//         }
//     }
// }