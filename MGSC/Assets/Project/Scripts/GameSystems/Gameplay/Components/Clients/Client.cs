using Features;
using System;
using System.Collections;
using Cooking;
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
        public Action             OnMealServed     { get; set; }
        public Action<Order>      OnNewOrder       { get; set; }
        public Action<Order>      OnOrderAccepted  { get; set; }
        public Action<ClientSeat> OnSeatRegister   { get; set; }
        public Action<ClientSeat> OnSeatUnregister { get; set; }
        public ClientBehaviour    Behaviour        { get; set; }
        public ClientCooked       Cooked           { get; private set; }
        public ClientLook         ClientLook       => _clientLook;
        
        public bool               IsCooked         { get; set; }
        
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
                    Debug.Log("SITTING");
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

 
       public void CookClient()
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

       public void SittingClient(Transform seatTransform)
       {
           transform.SetParent(seatTransform);
           Behaviour.ClientMovement.Sit();
           StartCoroutine(SmoothTransformReset());
           Behaviour.OnSitting?.Invoke();
       }

       public void QuitSeat()
       {
           transform.SetParent(null);
           Behaviour.ClientMovement.GetUp();
           ClientLook.SetClientLeaving(this);
       }
       
       IEnumerator SmoothTransformReset()
       {
           float      elapsed  = 0;
           float      duration = 0.5f;
           Quaternion startRot = transform.localRotation;
           Vector3 startPos = transform.localPosition;
           while (elapsed < duration)
           {
               elapsed += Time.deltaTime;
               transform.localRotation = Quaternion.Lerp(startRot, Quaternion.Euler(0, 0, 0), elapsed/duration);
               transform.localPosition = Vector3.Lerp(startPos, Vector3.zero, elapsed/duration);

               yield return null;
           }
           transform.localRotation = Quaternion.Euler(0, 0, 0);
           transform.localPosition = Vector3.zero;
       }

       #endregion

       #region Order

       public void TryAcceptOrder(Order order)
       {
           if (order == null) return;
           if (CurrentOrder != null) return;
           CurrentOrder = order;
           //CurrentSeat.TryUnregister(this);
           OnOrderAccepted?.Invoke(order);
       }
       
       public void TryAcceptServing(Meal meal)
       {
           if (meal == null) return;
           CurrentOrder.Resolve(meal);
           CurrentSeat?.TryLeave(this);
           OnMealServed?.Invoke();
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
