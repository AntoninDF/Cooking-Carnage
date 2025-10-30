using Cooking;
using GameSystem;
using Orders;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
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
        public SO_Client Model { get; private set; }
        public float Speed { get => _speed; set => _speed = value; }
        public bool IsMoving => _state == EClientState.Moving;
        public bool IsOrdering => _state == EClientState.Ordering;
        public bool IsSitting => _state == EClientState.Sitting;
        public float ReachDistance { get => _reachDistance; set => _reachDistance = value; }
        public bool HasOrdered => _hasOrdered;

        #endregion

        #region Fields
        private float _speed = 1f;
        private SplineContainer _splineComp;
        private List<Counter> _counters;

        private float _reachDistance = 1.25f;
        private Counter _currentCounter;

        // It's not a clean state machine for now. To be refactored later, probably. It's quite messy as is
        private EClientState _state = EClientState.None;
        private bool _hasOrdered = false;

        private ClientMovement _movement;
        private bool _debugSplinePosition;
        private Order _order;

        private GameLoop _gameLoop;
        #endregion

        #region Methods

        private void Start()
        {
            _movement = new(transform, _speed, _splineComp);

            _counters = ReferenceManager.Instance.GetElementsOfType<Counter>();
            if (_counters == null || _counters.Count == 0)
                Debug.LogError("[Client] Start : No counters were found. Please place at least one of each in the scene.");

            if (_splineComp == null)
                Debug.LogWarning("[Client] Init : No spline component was referenced. Consider using SetSplinePath.");

            _movement.SetSplinePos(0.0f);

            _gameLoop = ReferenceManager.Instance.GetManager<GameLoop>();
            if (_gameLoop == null)
                Debug.LogError("[Client] Init : No gameloop found.");

        }

        private void Update()
        {
            if (_gameLoop == null || !_gameLoop.IsRunning) return;

            switch (_state)
            {
                case EClientState.None :

                    if (_currentCounter)
                    {
                        _state = EClientState.Moving;
                    }
                    
                    break;
                
                case EClientState.Moving:
                    
                    if(!_currentCounter){ break; }
                    
                    _movement.MoveAlongSpline();
                    
                    if (!_hasOrdered)
                    { 
                        if (CheckClientCounterDistance(_currentCounter))
                        {
                            _state = EClientState.AtCounter;
                            UseCounter(_currentCounter);
                            
                            // le currentCounter est forcément occupé par this 
                            /*if (!_currentCounter.IsFree)
                            {
                                _state = EClientState.None;
                            }*/

                            break;
                        }
                    }
                    else if (CheckClientCounterDistance(_currentCounter))
                    {
                        _state = EClientState.AtCounter;
                        UseCounter(_currentCounter);
                    }
                    break;
                
                case EClientState.AtCounter:
                    
                    _movement.GoTo(_currentCounter.UserSpot.position);
                    
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
        }

        public void UseCounter(Counter counter)
        {
            counter.Interaction();
        }

        public void AcceptOrder(Order order)
        {
            if (CheckForCounter(out SittingCounter counter))
            {
                ReserveCounter(counter);
            }
            else
            {
                return; // Pas de Sitting dispo
            }
            
            _hasOrdered = true;
            _state = EClientState.ReachingSpline;
            _order = order;
        }

        public void ResolveOrder()
        {
            if (_state == EClientState.AtCounter && _hasOrdered)
            {
                //Destroy(gameObject);
                gameObject.SetActive(false);
                ReferenceManager.Instance.GetManager<ClientManager>().AddClientData(this,true,false);
            }
        }

        public void Kill()
        {
            //Destroy(gameObject);
            gameObject.SetActive(false);
            ReferenceManager.Instance.GetManager<ClientManager>().AddClientData(this,true,true);
        }

        public void AutoDestruction()
        {
            Destroy(gameObject);
        }


        public void OnDrawGizmos()
        {
            if (!_debugSplinePosition) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_movement.FindNearestSplinePosition(), 0.1f);

            Gizmos.color = Color.white;
            foreach (var pos in _movement.CheckedPositions)
            {
                Gizmos.DrawSphere(pos, 0.02f);
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

        }


    }
    
}