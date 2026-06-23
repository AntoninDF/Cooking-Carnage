using Features;
using System;
using GameSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Clients
{
    /// <summary>
    /// This part of the client class handles the client's behaviour.
    /// Movement is handled externally : See ClientMovement.cs
    /// The client generation is defined in ClientGenerator.cs. It is joined to the Client through partial class
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public partial class Client : MonoBehaviour, IFeatureCollection
    {
        #region Properties
        public FeatureCollection Collection { get; set; } = new();
        public Action<Order> OnNewOrder {  get; set; }
        public Action<ClientSeat> OnSeatRegister { get; set; }
        public Action<ClientSeat> OnSeatUnregister { get; set; }
        public ClientBehaviour Behaviour { get; set; }
        public Order CurrentOrder
        {
            get => _currentOrder;
            set
            {
                //Debug.Log("Current order is accessed");
                if (_currentOrder == value) return;
                _currentOrder = value;
                if (value != null)
                {
                    //Debug.Log("Current order is set");
                    OnNewOrder?.Invoke(value);
                }
            }
        }
        public ClientSeat CurrentSeat
        {
            get => _currentSeat;
            set
            {
                if (_currentSeat == value) return;
                OnSeatUnregister?.Invoke(_currentSeat);
                if (value != null)
                {
                    OnSeatRegister?.Invoke(value);
                }

                _currentSeat = value;
            }
        }

        #endregion

        #region Fields
        Order _currentOrder;
        ClientSeat _currentSeat;
        
        private float _splineExitT       = -1f; 
        private bool  _returningOnSpline = false;
        private float _waitT             = 0.9f;
        
        
        #endregion

        private void Awake()
        {
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            Behaviour = new(this, agent);
            // OnNewOrder += TryAcceptOrder;
            OnSeatRegister += OnNewSeatRegister;
            //_movement      =  new ClientMovement(transform, _speed, _splineComp);
        }

        private void Start()
        {
            // When a client appears, he is hungry by nature.
            if (CurrentOrder == null)
            {
               // Behaviour.GoToOrderingCounter(Sit);
                
                bool found = Behaviour.GoToOrderingCounter(Sit);
                if (!found)
                {
                    _waitT = CalculateWaitPosition();
                }
            }
           
        }

        void Update()
        {
            Collection.Update();

            if (_movement == null || _splineComp == null) return;

            if (_returningOnSpline)
            {
                _movement.MoveAlongSpline();
                if (_movement.SplinePosition >= 1f)
                {
                    GameObject.Destroy(gameObject);
                }
                return;
            }

            ClientSeat targetSeat = CurrentSeat;
            if (targetSeat != null)
            {
                float nearestT = GetNearestTOnSpline(targetSeat.transform.position);
    
                if (_movement.SplinePosition >= nearestT)
                {
                    _splineExitT = _movement.SplinePosition;
                    _splineComp  = null;
                    Behaviour.ClientMovement.SetDestination(targetSeat);
                    return;
                }
            }
            else
            {
                if (_movement.SplinePosition >= _waitT)
                    return;
            }

            _movement.MoveAlongSpline();

            _movement.MoveAlongSpline();
        }

        public void Sit(ClientSeat seat)
        {
            if (seat == null) return;

            seat.TrySit(this);
        }

        private void OnNewSeatRegister(ClientSeat seat)
        {
            if (seat.TryGetSibling(out OrderingCounter orderingCounter))
            {
                if (CurrentOrder == null)
                {
                    //orderingCounter.OnNewOrder += TryAcceptOrder;
                    return;
                }
            }
           //if (seat.TryGetSibling(out ServingCounter servingCounter))
           //{
           //    servingCounter.OnServe += TryAcceptServing;
           //}
        }

        public void TryAcceptOrder(Order order)
        {
            if (order == null) return;
            if (CurrentOrder != null) return;
            //Debug.Log("Order accepted");
            CurrentOrder = order;
            CurrentSeat.TryUnregister(this);
            Behaviour.GoToServingCounter(Sit);
        }
        
        private void TryAcceptServing(Servable serving)
        {
            if (serving == null || !serving.Enabled) return;
            CurrentOrder.Resolve(serving);
            CurrentSeat?.TryUnregister(this);
            StartReturn(); 
        }
        private void OnOrderResolution()
        {
            
        }
        
        public void StartReturn()
        {
            _returningOnSpline = true;
            _splineComp        = _movement.SplineComponent; 
            _movement.SetSplinePos(_splineExitT);
    
            var agent                        = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false;
        }

        private float GetNearestTOnSpline(Vector3 worldPosition)
        {
            return _movement.FindNearestSplineT(worldPosition);
        }

        
        private float CalculateWaitPosition()
        {
            var allSeats = Reference.GetManager<ClientManager>()
                                    .GetClientSeats(_ => true, mustBeAvailable: false);
    
            float closestT = 0.9f;
            float minDist  = float.MaxValue;

            foreach (var seat in allSeats)
            {
                if (seat?.transform == null) continue;
                float t    = GetNearestTOnSpline(seat.transform.position);
                float dist = Vector3.Distance(
                                              _movement.GetSplinePos(t), 
                                              seat.transform.position
                                             );
                if (dist < minDist)
                {
                    minDist  = dist;
                    closestT = t;
                }
            }
            
            int   waitingCount = CountClientsWaiting();
            float offset       = 0.05f + waitingCount * 0.04f;
            return Mathf.Clamp(closestT - offset, 0f, 1f);
        }

        private int CountClientsWaiting()
        {
            int count         = 0;
            var clientManager = Reference.GetManager<ClientManager>();
            foreach (var client in clientManager.CurrentClients)
            {
                if (client != this && client.CurrentSeat == null && client.Movement != null)
                    count++;
            }
            return count;
        }
        
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            // Draw order
            if (CurrentOrder != null)
            {
                Gizmos.color = Color.white;
                Handles.Label(transform.position + Vector3.up * 1f, "HAS ORDER");
            }
            else
            {
                Gizmos.color = Color.Lerp(Color.white, Color.red, 0.3f);
                Handles.Label(transform.position + Vector3.up * 1f, "NO ORDER");
            }

            if (CurrentSeat != null)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.blue, 0.3f);
                Handles.Label(transform.position + Vector3.up * 1.2f, "CurrentSeat is " + (CurrentSeat.Owner as UnityEngine.Object).name);
            }
        }
        // Find ClientTargets



        /*

        The client looks for seats.
        If none are found, they stay in place.
        If there are seats available, the client asks themselves : Am I hungry ?
        If so, they look for a seat associated to a OrderingCounter
        Otherwise they select another seat.


        */
    }
}
