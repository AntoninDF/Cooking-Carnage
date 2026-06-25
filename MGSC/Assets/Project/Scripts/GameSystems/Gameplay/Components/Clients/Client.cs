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
        public FeatureCollection  Collection       { get; set; } = new();
        public Action<Order>      OnNewOrder       { get; set; }
        public Action<Order>      OnOrderAccepted  { get; set; }
        public Action<ClientSeat> OnSeatRegister   { get; set; }
        public Action<ClientSeat> OnSeatUnregister { get; set; }
        public ClientBehaviour    Behaviour        { get; set; }
        public ClientCooked       Cooked           { get; private set; }

        public Order CurrentOrder
        {
            get => _currentOrder;
            set
            {
                if (_currentOrder == value) return;
                _currentOrder = value;
                if (value != null)
                    OnNewOrder?.Invoke(value);
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
                }
                _currentSeat = value;
            }
        }

       #endregion

        #region Fields
       
        private Order      _currentOrder;
        private ClientSeat _currentSeat;
        private ClientLook _clientLook;

        private float _cookingDelay = 3f;
       #endregion

       #region Unity Lifecycle

       private void Awake()
       {
           NavMeshAgent agent = GetComponent<NavMeshAgent>();
           Behaviour   = new ClientBehaviour(this, transform, agent, _splineComp, _speed);
           
           OnSeatRegister += OnNewSeatRegister;
       }

       public void Begin()
       {
           Behaviour.Begin();
       }
       void Update()
       {
           Collection.Update();
           Behaviour.Update();
       }

       #endregion

       public void InitFeatures(ClientMovement movement)
       {
           _clientLook = new ClientLook(this, transform, movement);
           Cooked      = new ClientCooked(this, _cookingDelay);
           Collection.Add(_clientLook);
           Collection.Add(Cooked);
       }

       [ContextMenu("Cook Client")]
       public void CoolClient()
       {
           Cooked.Trigger();
       }

       #region Seat

       public void OnNewSeatRegister(ClientSeat seat)
       {
           if (seat == null) return;

           if (seat.TryGetSibling(out OrderingCounter orderingCounter))
           {
               if (CurrentOrder == null)
               {
                   orderingCounter.OnNewOrder += TryAcceptOrder;
                   return;
               }
           }
           
       }

       #endregion

       #region Order

       public void TryAcceptOrder(Order order)
       {
           if (order == null) return;
           if (CurrentOrder != null) return;
           CurrentOrder = order;
           CurrentSeat.TryUnregister(this);
           OnOrderAccepted?.Invoke(order);
       }

       private void TryAcceptServing(Servable serving)
       {
           if (serving == null || !serving.Enabled) return;
           CurrentOrder.Resolve(serving);
           CurrentSeat?.TryUnregister(this);
       }

       #endregion
       
       #region Gizmos

       private void OnDrawGizmos()
       {
           if (!Application.isPlaying) return;

           if (CurrentOrder != null)
           {
               Gizmos.color = Color.white;
               UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, "HAS ORDER");
           }
           else
           {
               Gizmos.color = Color.Lerp(Color.white, Color.red, 0.3f);
               UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, "NO ORDER");
           }

           if (CurrentSeat != null)
           {
               Gizmos.color = Color.Lerp(Color.white, Color.blue, 0.3f);
               UnityEditor.Handles.Label(
                                         transform.position + Vector3.up * 1.2f,
                                         "CurrentSeat is " + (CurrentSeat.Owner as UnityEngine.Object)?.name);
           }
           
           
       }

       #endregion
       
    }
}
