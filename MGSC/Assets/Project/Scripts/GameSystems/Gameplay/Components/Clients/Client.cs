using Features;
using System;
using System.Collections;
using System.Collections.Generic;
using Cooking;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

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
        public ClientLook         ClientLook       => _clientLook;
        
        public bool               IsCooked         { get; set; }
        public int UniqueID { get; set; }
        
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
                    //Debug.Log("SITTING");
                }
                _currentSeat = value;
            }
        }

       #endregion

        #region Fields
       
        private Order                 _currentOrder;
        private ClientSeat            _currentSeat;
        private ClientLook            _clientLook;


        private float _cookingDelay = 3f;
       #endregion

       #region Unity Lifecycle

       private void Awake()
       {
           NavMeshAgent agent = GetComponent<NavMeshAgent>();
           Behaviour = new ClientBehaviour(this, transform, agent, _splineComp, _speed);
           
           OnSeatRegister += OnNewSeatRegister;
           UniqueID = gameObject.GetInstanceID();
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

       public void SpawnIngredients(List<SlotContainer> containers)
       {
           List<SlotContainer> positions = containers;
           int numberToDrop = Random.Range((int)DropRate.x, (int)DropRate.y);

           for (int i = 0; i < numberToDrop; i++)
           {
               Instantiate(Model.loot.Prefab, positions[i].transform);
           }
       }

       #region Seat
       
       /// <summary>
       /// Executed when a new Seat is registered (in general ta the spawn)
       /// Or when the wait line have a nex seat free
       /// </summary>
       /// <param name="seat"></param>
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

       /// <summary>
       /// Move the customer to his place on the seat and parenting hil to the seat
       /// </summary>
       /// <param name="seatTransform"></param>
       public void SittingClient(Transform seatTransform)
       {
           transform.SetParent(seatTransform);
           Behaviour.ClientMovement.Sit();
           StartCoroutine(SmoothTransformReset());
           Behaviour.OnSitting?.Invoke();
       }

       /// <summary>
       /// Unparenting the customer and set his moves and look for quitting
       /// </summary>
       public void QuitSeat()
       {
           transform.SetParent(null);
           Behaviour.ClientMovement.GetUp();
           ClientLook.SetClientLeaving(this);
       }
       
       /// <summary>
       /// Smooth Lerping to the seat place when the navmesh reach the seat 
       /// </summary>
       /// <returns></returns>
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

       /// <summary>
       /// Assign an order to the custommer
       /// </summary>
       /// <param name="order"></param>
       public void TryAcceptOrder(Order order)
       {
           if (order == null) return;
           if (CurrentOrder != null) return;
           CurrentOrder = order;
           OnOrderAccepted?.Invoke(order);
       }
       
       /// <summary>
       /// If the customer is served, check if the recipe is good to be evaluate
       /// and evaluate it if he can
       /// </summary>
       /// <param name="serving"></param>
       public void TryAcceptServing(Servable serving)
       {
           if (serving == null || !serving.Enabled) return;
           if (CurrentOrder == null) return;

           var recipeComponent = serving.Owner as Recipe;
           if (recipeComponent == null) return;

           SO_Recipe soRecipe = recipeComponent.RecipeFeature?.Recipe;
           if (soRecipe == null) return;

           var info = new Orders.ServingInfo(soRecipe);

           var orderManager = Reference.GetManager<GameSystem.OrderManager>();
           orderManager.ResolveOrder(CurrentOrder, info);

           CurrentSeat?.TryUnregister(this);
       }

        #endregion

        #region Gizmos

#if UNITY_EDITOR
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
#endif
#endregion

    }
}
