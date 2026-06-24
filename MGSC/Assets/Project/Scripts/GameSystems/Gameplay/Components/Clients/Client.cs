using Features;
using System;
using GameSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;

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
                   value.OnClientSitting += _clientLook.SetClientSitting;
                   value.OnClientSitting += DebugSeat;
               }

               _currentSeat = value;
           }
       }

       #endregion

       #region Fields
       
       private Order _currentOrder;
       private ClientSeat _currentSeat;
       private ClientLook _clientLook;
       
       private float _splineExitTime       = -1f; 
       private bool  _returningOnSpline = false;
       private float _waitTime             = 0.9f;
       
       
       #endregion

        public void DebugSeat(Client client)
        {
            Debug.Log(client.name);
        }
        private void Awake()
        {
            OnSeatRegister += OnNewSeatRegister;
        }

        private void Start()
        {
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            Behaviour = new(this, transform, agent, _splineComp, _speed);
            _clientLook = new(this, transform);
            Collection.Add(_clientLook);
            
            //Not he client to decide
            //decaler ça dans une Behavior Action
            
            bool found = Behaviour.GoToOrderingCounter(Sit);
            if (!found)
            {
                var queue = Reference.GetManager<ClientManager>().WaitingQueue;
                _waitTime = queue.Enqueue(this);
            }
            
        }

        
        void Update()
        {
            Collection.Update();

            //Ce n'est pas non plus le role du client 
            //ClientMovement s'occupe d'appliquer le movement
            //le Behavior action s'occupes des choix
            //Client doit soccuper grand max d'executer
            
            if (Behaviour.ClientMovement == null || _splineComp == null) return;

            if (_returningOnSpline)
            {
                Behaviour.ClientMovement.MoveAlongSpline();
                if (Behaviour.ClientMovement.SplinePosition >= 1f)
                {
                    Destroy(gameObject);
                }
                return;
            }

            ClientSeat targetSeat = CurrentSeat;
            float currentT = Behaviour.ClientMovement.SplinePosition;
            if (targetSeat != null)
            {
                Vector3 blendedPos = targetSeat.transform.position - transform.position;
                float nearestT = GetNearestTOnSpline(targetSeat.transform.position) - 0.05f;
                if (currentT >= nearestT)
                {
                    _splineExitTime = Behaviour.ClientMovement.SplinePosition;
                    _splineComp  = null;
                    Behaviour.ClientMovement.SetDestination(targetSeat);
                    _clientLook.SetTarget((blendedPos).normalized);
                    return;
                }

            }
            else
            {
                if (Behaviour.ClientMovement.SplinePosition >= _waitTime)
                {
                    
                    return;
                }
            }
            
            _clientLook.SetTarget(((Behaviour.ClientMovement.GetSplinePos(currentT + 0.03f) - transform.position)).normalized);
            Behaviour.ClientMovement.MoveAlongSpline();
            
        }
        
        //Peux être mis dans une Action du behavior 
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
                    orderingCounter.OnNewOrder += TryAcceptOrder;
                    return;
                }
            }
           if (seat.TryGetSibling(out ServingCounter servingCounter))
           {
               servingCounter.OnServe += TryAcceptServing;
           }
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
        
        //Logique à bouger dans ClientMovement
        public void StartReturn()
        {
            _returningOnSpline = true;
            _splineComp        = Behaviour.ClientMovement.SplineComponent; 
            Behaviour.ClientMovement.SetSplinePos(_splineExitTime);
    
            var agent                        = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false;
        }

        private float GetNearestTOnSpline(Vector3 worldPosition)
        {
            return Behaviour.ClientMovement.FindNearestSplineT(worldPosition);
        }

        
        //
        // private int CountClientsWaiting()
        // {
        //     int count         = 0;
        //     var clientManager = Reference.GetManager<ClientManager>();
        //     foreach (var client in clientManager.CurrentClients)
        //     {
        //         if (client != this && client.CurrentSeat == null && Behaviour.ClientMovement != null)
        //             count++;
        //     }
        //     return count;
        // }
        
        public void SetWaitT(float t)
        {
            _waitTime = t;
        }

        public void AssignSeat(ClientSeat seat)
        {
            seat.TryRegister(this); 
            _waitTime = -1f; 
            Behaviour.ClientMovement.SetDestination(seat);
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
