using UnityEngine;
using UnityEngine.AI;

namespace Features
{
    public class ClientMovement : Feature
    {
        public ClientMovement(IFeatureCollection owner, NavMeshAgent agent) : base(owner)
        {
            Agent = agent;
        }

        NavMeshAgent Agent { get; set; }

        public void SetDestination(Vector3 worldPosition)
        {
            Agent.SetDestination(worldPosition);
        }
        public void SetDestination(ClientSeat seat)
        {
            if (seat == null)
            {
                Debug.LogWarning("ClientMovement.SetDestination : Seat is null");
                return; 
            }
            Agent.SetDestination(seat.transform.position);
        }
    }
}