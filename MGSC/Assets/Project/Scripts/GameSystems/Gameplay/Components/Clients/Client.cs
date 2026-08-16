using Features;
using System;
using System.Collections;
using System.Collections.Generic;
using Cooking;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using UnityEditor;
using System.Linq;
using GameSystem;

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
        public Action<float> OnOrderTimeProgress { get; set; }
        [GlobalEvent("Clients/On Client Enters The Cafeteria")]
        public static Action OnEnterCafeteria;
        public ClientBehaviour    Behaviour        { get; set; }
        public ClientCooked       Cooked           { get; private set; }
        public ClientLook         ClientLook       => _clientLook;
        
        public bool               IsCooked         { get; set; }
        public bool               IsAngry          { get; set; }
        public int UniqueID { get; set; }
        public Timer PatienceTimer;
        
        public Order OrderLog { get;  set; }
        public Order CurrentOrder
        {
            get => _currentOrder;
            set
            {
                if (_currentOrder == value) return;
                _currentOrder = value;
                if (value != null)
                {
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
           SubscribeToUpgrade();
           NavMeshAgent agent = GetComponent<NavMeshAgent>();
           Behaviour = new ClientBehaviour(this, transform, agent, _splineComp, _speed);
           
           OnSeatRegister += OnNewSeatRegister;
           UniqueID = gameObject.GetInstanceID();

        }

#if UNITY_EDITOR && EXTENSIVE_DEBUG
        // The following block is for debug purposes only
        void OnNewOrderLog(Order obj)               => Debug.Log($"[Client {name}] OnNewOrder          " + " was invoked" + " with parameter " + (obj != null ? obj.Recipe.name : "'null'"));
        void OnOrderAcceptedLog(Order obj)          => Debug.Log($"[Client {name}] OnOrderAccepted     " + " was invoked" + " with parameter " + (obj != null ? obj.Recipe.name : "'null'"));
        void OnSeatRegisterLog(ClientSeat obj)      => Debug.Log($"[Client {name}] OnSeatRegister      " + " was invoked" + " with parameter " + (obj != null ? (obj.Owner as UnityEngine.Object).name : "'null'"));
        void OnSeatUnregisterLog(ClientSeat obj)    => Debug.Log($"[Client {name}] OnSeatUnregister    " + " was invoked" + " with parameter " + (obj != null ? (obj.Owner as UnityEngine.Object).name : "'null'"));
        void OnEnable() => BindActionLogs();
        void OnDisable() => UnbindActionLogs();
        void BindActionLogs()
        {
            OnNewOrder              += OnNewOrderLog;
            OnOrderAccepted         += OnOrderAcceptedLog;
            OnSeatRegister          += OnSeatRegisterLog;
            OnSeatUnregister        += OnSeatUnregisterLog;
        }
        void UnbindActionLogs()
        {
            OnNewOrder              -= OnNewOrderLog;
            OnOrderAccepted         -= OnOrderAcceptedLog;
            OnSeatRegister          -= OnSeatRegisterLog;
            OnSeatUnregister        -= OnSeatUnregisterLog;
        }

        // This huge debug block should be removed later
        float elapsedSinceLastLog = 0f;
#endif
        void Update()
        {
           Collection.Update();
           Behaviour.Update();
#if UNITY_EDITOR && EXTENSIVE_DEBUG
            float logCooldown = 2f;
            elapsedSinceLastLog += Time.deltaTime;
            if (elapsedSinceLastLog >= logCooldown)
            {
                elapsedSinceLastLog -= logCooldown;
                Log();
            }

            void Log()
            {
                Debug.Log("__________ " + GetType().Name + " : " + name + " __________");

                Debug.Log("Client type" + " : " + Type);
                Debug.Log("Current box position" + " : " + (CurrentSeat != null ? ((CurrentSeat.Owner as UnityEngine.Object).name.Contains("1") ? "Middle" : (CurrentSeat.Owner as UnityEngine.Object).name.Contains("2") ? "Right" : "Left") : "null"));
                Debug.Log("Order" + " : " + (CurrentOrder != null ? (CurrentOrder.Recipe.name + " (Has" + (CurrentOrder.ServingInfo.HasBeenServed ? " not" : "") + " been served)") : "null"));
                if (CurrentOrder != null && CurrentOrder.ServingInfo.HasBeenServed)
                    Debug.Log("Serving" + " : " + CurrentOrder.ServingInfo.Recipe.name+ ", " + CurrentOrder.OrderRating.MoneyGained + "€, " + CurrentOrder.OrderRating.Stars + " stars");
                Debug.Log("Patience" + " : " + (PatienceTimer == null ? 0f : PatienceTimer.Elapsed) + "s/" + (PatienceTimer == null ? 0f : PatienceTimer.Duration));

                Debug.Log("__________ " + "##########" + " __________");

            }
#endif
        }

        public void Begin()
       {
           Behaviour.Begin();
       }

        #endregion
        public void BeginOrderTimer()
        {
            if (CurrentOrder == null) return;
            if (_currentOrder != null) _currentOrder.SeatedTimestamp = Time.time;
            StartOrderTimer(CurrentOrder);
        }

        private void StartOrderTimer(Order order)
        {
            string timerName = "OrderTimer_" + UniqueID;
            float duration = order.PatienceDuration; // This variable is useful for unboxing purposes
            PatienceTimer = GameSystem.TimerManager.StartGameplayTimer(
                timerName,
                duration,
                () => OnOrderTimeout(order),
                elapsed => OnOrderTimeProgress?.Invoke(Mathf.Clamp01(elapsed / duration))
            );
        }

        private void OnOrderTimeout(Order order)
        {
            if (CurrentOrder != order) return;
            IsAngry = true;
            order.Cancel();
            CurrentSeat?.TryUnregister(this);
        }
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


       internal void Despawn()
       {
            if (Behaviour?.ClientMovement != null)
                Reference.GetManager<GameSystem.PathManager>()?.SplineTrafficManager.Unregister(Behaviour.ClientMovement);

            GameSystem.TimerManager.RemoveTimer(PatienceTimer);
            GameSystem.TimerManager.RemoveTimer($"Customer Eating Timer + {UniqueID}");
            GameSystem.TimerManager.RemoveTimer($"{UniqueID}Wait For Quit the seat");
            GameSystem.TimerManager.RemoveTimer($"{UniqueID} Wait For finishing the order ");
            GameSystem.TimerManager.RemoveTimer($"{UniqueID} Wait finishing the react animation");

            CurrentSeat?.TryUnregister(this);
            Destroy(gameObject);
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
                   OnEnterCafeteria?.Invoke();
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
               elapsed += GameLoop.IsRunning ? Time.deltaTime : 0;
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

           var info = new Orders.ServingInfo(serving); 
           CurrentOrder.ServingInfo = info;

            Achievements.Unlock("ACH_A02");

           var orderManager = Reference.GetManager<GameSystem.OrderManager>();
           orderManager.ResolveOrder(CurrentOrder, info);
           GameSystem.TimerManager.RemoveTimer(PatienceTimer);
           CurrentSeat?.TryUnregister(this);
       }

        #endregion

        #region Gizmos

#if UNITY_EDITOR
        
        private void OnDrawGizmos()
        {
            float baseHandleHeight = 1f;
            float increment = 0.2f;
            var cam = SceneView.lastActiveSceneView.camera;
            if (cam != null) increment = Vector3.Distance(cam.transform.position, transform.position) / 50f;
            int handleCount = 0;
            float GetY()
            {
                handleCount++;
                return baseHandleHeight + (handleCount - 1) * increment;
            }
            if (!Application.isPlaying) return;

            if (CurrentOrder != null)
            {
               Gizmos.color = Color.white;
               UnityEditor.Handles.Label(transform.position + Vector3.up * GetY(), "HAS ORDER");
               UnityEditor.Handles.Label(transform.position + Vector3.up * GetY(), "Order contains " + CurrentOrder.Recipe.Ingredients.Select(ing => ing.Name).ToList().ToString_EasyRead());
            }
            else
            {
                Gizmos.color = Color.Lerp(Color.white, Color.red, 0.3f);
                UnityEditor.Handles.Label(transform.position + Vector3.up * GetY(), "NO ORDER");
            }

            if (CurrentSeat != null)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.blue, 0.3f);
                UnityEditor.Handles.Label(
                                            transform.position + Vector3.up * GetY(),
                                            "CurrentSeat is " + (CurrentSeat.Owner as UnityEngine.Object)?.name);
            }
            {
                Gizmos.color = Color.Lerp(Color.white, Color.blue, 0.3f);
                UnityEditor.Handles.Label(transform.position + Vector3.up * GetY(), "ClientType : " + Type);
            }
            {
                Gizmos.color = Color.Lerp(Color.white, Color.blue, 0.3f);
                UnityEditor.Handles.Label(transform.position + Vector3.up * GetY(), "Patience : " + TimerManager.TimeLeft("OrderTimer_" + UniqueID) + "s");
            }
        }

#endif
        #endregion

    }
}
