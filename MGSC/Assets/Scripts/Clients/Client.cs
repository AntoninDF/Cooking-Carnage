using Cooking;
using GameSystem;
using Orders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

namespace Clients
{
    /// <summary>
    /// This part of the client class handles the client's behaviour.
    /// Movement is handled externally : See ClientMovement.cs
    /// The client generation is defined in ClientGenerator.cs. It is joined to the Client through partial class
    /// </summary>
    public partial class Client : MonoBehaviour
    {
        #region Properties
        public EClientType Type { get; private set; }
        public SO_Recipe RequestedRecipe {  get; private set; }
        public Order Order => _order;
        public SO_Ingredient IngredientToDrop { get; private set; }
        public Vector2 DropRate { get; private set; }
        public SO_Client Model { get; private set; }
        public float Speed { get => _speed; set => _speed = value; }
        public bool IsMoving => _state == EClientState.Moving;
        public bool IsOrdering => _state == EClientState.Ordering;
        public bool IsSitting => _state == EClientState.Sitting;
        public float ReachDistance { get => _reachDistance; set => _reachDistance = value; }
        public bool HasOrdered => _hasOrdered;

        public Collider Collider => _collider;

        #endregion

        #region Fields
        [Header("Client parameters")]
        [SerializeField] private float _timeToEat = 2f;
        [SerializeField] private float _timeBeforeDisappearingAfterLeaving = 15f;
        [SerializeField] private float _timeBeforeDisappearingAfterDying = 3f;
        [Range(-1f, 0)] [SerializeField] private float deathTrapHeight = -1;

        [Header("Client events")]
        [SerializeField] private UnityEvent _onLookAt;
        [SerializeField] private UnityEvent _onStopLookAt;
        [SerializeField] private UnityEvent _onStartEating;
        [SerializeField] private UnityEvent _onEndEating;
        [SerializeField] private UnityEvent _onStartWalking;
        [SerializeField] private UnityEvent _onEndWalking;
        [SerializeField] private UnityEvent _onCounterRegister;
        [SerializeField] private UnityEvent _onCounterArrival;
        [SerializeField] private UnityEvent _onCounterDeparture;
        [SerializeField] private UnityEvent _onKilled;
        [SerializeField] private UnityEvent _onSatisfactoryOrder;
        [SerializeField] private UnityEvent _onWrongOrder;
        [SerializeField] private UnityEvent _onStart;


        private float _speed = 1f;
        private SplineContainer _splineComp;
        private List<Counter> _counters;

        private float _reachDistance = 1.25f;
        private Counter _currentCounter;

        // It's not a clean state machine for now. To be refactored later, probably. It's quite messy as is
        private EClientState _state = EClientState.None;
        private bool _hasOrdered = false;
        private bool _hasEaten;

        private ClientMovement _movement;
        private bool _debugSplinePosition;
        private Order _order;

        private GameLoop _gameLoop;
        private bool _useUpgrades = true;

        private Collider _collider;
        private Vector3 _previousPos = Vector3.zero;
        private Action _tryResolveOrder = null;
        #endregion

        #region Methods

        private void Start()
        {
            _movement = new(transform, _speed, _splineComp);
            _collider = GetComponent<Collider>();

            _counters = ReferenceManager.Instance.GetElementsOfType<Counter>();
            if (_counters == null || _counters.Count == 0)
                Debug.LogError("[Client] Start : No counters were found. Please place at least one of each in the scene.");

            if (_splineComp == null)
                Debug.LogWarning("[Client] Init : No spline component was referenced. Consider using SetSplinePath.");

            _movement.SetSplinePos(0.0f);
            
            _onStart?.Invoke();

            _gameLoop = ReferenceManager.Instance.GetManager<GameLoop>();
            if (_gameLoop == null)
                Debug.LogError("[Client] Init : No gameloop found.");
        }

        private void Update()
        {
            if (_gameLoop == null || !_gameLoop.IsRunning) return;

            switch (_state)
            {
                case EClientState.None:
                    
                    break;

                case EClientState.Moving:

                    if (!_currentCounter) { break; }

                    _movement.MoveAlongSpline();

                    if (CheckClientCounterDistance(_currentCounter) && !_hasEaten)
                    {
                        _state = EClientState.AtCounter;
                        UseCounter(_currentCounter);
                    }
                    break;

                case EClientState.AtCounter:
                    
                    if (_currentCounter is OrderingCounter counter)
                    { 
                        counter.TryAcceptOrder();
                    }
                    else
                    {
                        _state = EClientState.PlatformElevation;
                        _currentCounter.GetComponent<SittingCounter>().HeightDeathTrap(deathTrapHeight); 
                        _currentCounter.GetComponent<SittingCounter>().OnPlatformUp?.Invoke(); 
                        _onCounterArrival?.Invoke();
                    }
                    break;

                case EClientState.ReachingSpline:

                    Vector3 nearestPos = _movement.FindNearestSplinePosition();
                    _movement.GoTo(nearestPos);
                    if (Vector3.Distance(transform.position, nearestPos) <= 0.03f)
                    {
                        _state = EClientState.Moving;
                        _movement.SnapToSpline();
                    }
                    break;
                
                case EClientState.PlatformElevation:

                    Vector3 sizeMattersUserSpot = _currentCounter.UserSpot.position;
                    if (sizeMattersUserSpot.y < 0)
                    {
                        sizeMattersUserSpot.y = 0;
                    }
                    
                    if (transform.position != sizeMattersUserSpot || _previousPos != sizeMattersUserSpot)
                    {
                        _movement.GoTo(sizeMattersUserSpot);
                    }
                    else
                    {
                        transform.forward = _currentCounter.UserSpot.forward;
                        _state = EClientState.None;
                        if (_tryResolveOrder != null)
                        {
                            _tryResolveOrder();
                            _tryResolveOrder = null;
                        }
                    }
                    break;
            }

            FaceDirection();
            _previousPos = transform.position;
        }
        

        private void FaceDirection()
        { 
            if (transform.position != _previousPos)
            {
                Vector3 direction = new Vector3(transform.position.x - _previousPos.x, 0, transform.position.z - _previousPos.z);
                if (direction != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
            
        }

        /// <summary>
        /// Detects if a free counter of type T is available nearby.
        /// </summary>
        /// <typeparam name="T">The specific counter type to look for</typeparam>
        /// <returns>The first free counter found</returns>
        private T CheckForCounter<T>(bool mustBeFree=true) where T : Counter
        {
            _counters = ReferenceManager.Instance.GetElementsOfType<Counter>();
            T closestCounter = null;
            float closestDistance = float.MaxValue;
            foreach (var counter in _counters)
            {
                if (counter == null || counter is not T ) continue;
                
                float distance = Vector3.Distance(transform.position, counter.UserSpot.position); 

                if (distance < closestDistance)
                {
                    if (!mustBeFree || counter.IsFree)
                    {
                        closestDistance = distance;
                        closestCounter = counter as T;
                    }
                }
            }
            return closestCounter;
        }
        /// <summary>
        /// Detects if a free counter of type T is available nearby.
        /// </summary>
        /// <typeparam name="T">The specific counter type to look for</typeparam>
        /// <returns>True if a counter is found, false otherwise</returns>
        private bool CheckForCounter<T>(out T counter, bool mustBeFree = true) where T : Counter
        {
            counter = CheckForCounter<T>(mustBeFree);
            return counter != null;
        }

        public bool CheckClientCounterDistance(Counter counter)
        {
            return Vector3.Distance(transform.position, counter.transform.position) < Mathf.Max(_reachDistance, counter.ReachDistance);
        }

        public void ReserveCounter(Counter counter)
        {
            _currentCounter = counter;
            _currentCounter.Register(this);
            _onCounterRegister?.Invoke();
            
            _state = EClientState.Moving;
        }

        public void UseCounter(Counter counter)
        {
            counter.Interaction();
        }

        public void AcceptOrder(Order order)
        {
            if (CheckForCounter(out SittingCounter counter))
            {
                _hasOrdered = true;
                _state = EClientState.ReachingSpline;
                _order = order;
                //Debug.Log("[Client] : Order is " + _order);
                ReserveCounter(counter);
            }
            else
            {
                return; // Pas de Sitting dispo
            }
        }

        public void ResolveOrder(ServingInfo info, OrderRating rating)
        {
            if (_state == EClientState.None && _hasOrdered && _currentCounter)
            {
                _onStartEating?.Invoke();

                var tm = ReferenceManager.Get<TimerManager>();
                if (tm != null)
                    tm.StartTimer("Eating", 2f, () => Eat(info, rating));
                ReferenceManager.Instance.GetManager<ClientManager>().AddClientData(this,true,false);
            }
            else
            {
                _tryResolveOrder = () => ResolveOrder(info, rating);
            }
        }

        private void Eat(ServingInfo info, OrderRating rating)
        {
            _onEndEating?.Invoke();
            if (rating.IsCorrectRecipe)
                _onSatisfactoryOrder?.Invoke();
            else
                _onWrongOrder?.Invoke();

            //Refacto : Faire que le client mange et parte du resto
            var tm = ReferenceManager.Get<TimerManager>();
            if (tm != null)
            {
                tm.StartTimer("Leave", _timeToEat, () => LeaveCounterFeedback() );
                tm.StartTimer("Disable client", _timeBeforeDisappearingAfterLeaving, () => gameObject.SetActive(false)); //timer aussi utilisé dans endshift shiftmanager
            }
            _hasEaten = true;
        }

        private void LeaveCounterFeedback()
        {
            _onCounterDeparture?.Invoke();
            _currentCounter.GetComponent<SittingCounter>().OnPlatformDown?.Invoke(); 
            _state = EClientState.ReachingSpline;
        }

        public void Kill()
        {
            _onKilled?.Invoke(); 

            var tm = ReferenceManager.Get<TimerManager>();
            if (tm != null)
                tm.StartTimer(gameObject.name + " Dying (ID:" + Time.time.ToString() + ")", _timeBeforeDisappearingAfterDying, () => gameObject.SetActive(false));
            
            ReferenceManager.Instance.GetManager<ClientManager>().AddClientData(this,true,true);
        }

        public void AutoDestruction()
        {
            Destroy(gameObject);
        }

        public void OnDrawGizmos()
        {
            if (_debugSplinePosition)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(_movement.FindNearestSplinePosition(), 0.1f);

                Gizmos.color = Color.white;
                foreach (var pos in _movement.CheckedPositions)
                {
                    Gizmos.DrawSphere(pos, 0.02f);
                }
            }

        }

        #endregion

        private enum EClientState
        {
            None = 0,
            Moving = 1,
            Ordering = 2,
            Sitting = 3,
            ReachingSpline = 4,
            AtCounter = 5,
            PlatformElevation = 6,

        }


    }
    
}