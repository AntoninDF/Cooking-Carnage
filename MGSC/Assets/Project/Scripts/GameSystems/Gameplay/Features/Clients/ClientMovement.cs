using System;
using UnityEngine;
using UnityEngine.AI;

namespace Features
{
    public class ClientMovement : Feature
    {
        #region Properties
        public bool IsMoving
        {
            get => _isMoving;
            set
            {
                if (value == _isMoving) return;
                _isMoving = value;
                if (_isMoving) OnStartMoving?.Invoke();
                else OnStopMoving?.Invoke();
            }
        }
        public Action OnDestinationReached { get; set; }
        public Action OnStartMoving { get; set; }
        public Action OnStopMoving { get; set; }

        NavMeshAgent Agent { get; set; } 
        #endregion


        private bool _isMoving;


        #region Methods
        public ClientMovement(IFeatureCollection owner, NavMeshAgent agent) : base(owner)
        {
            Agent = agent;
        }
        public void SetDestination(Vector3 worldPosition)
        {
            Agent.SetDestination(worldPosition);
            IsMoving = true;
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
        public override void Update()
        {
            base.Update();
            HasReachedDestination();
        }

        public bool HasReachedDestination()
        {
            // https://discussions.unity.com/t/how-can-i-tell-when-a-navmeshagent-has-reached-its-destination/52403/5
            // Check if we've reached the destination
            if (!Agent.pathPending)
            {
                if (Agent.remainingDistance <= Agent.stoppingDistance)
                {
                    if (!Agent.hasPath || Agent.velocity.sqrMagnitude == 0f)
                    {
                        if (IsMoving) OnDestinationReached?.Invoke();
                        IsMoving = false;
                        return true;
                    }
                }
            }
            return false;
        } 
        #endregion
    }
}