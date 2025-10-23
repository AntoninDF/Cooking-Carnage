using Cooking;
using GameSystem;
using Orders;
using System.Collections.Generic;
using System.Data;
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
        public List<SO_Ingredient> IngredientsToDrop { get; private set; }
        public SO_Client Model { get; private set; }
        public float Speed { get => _speed; set => _speed = value; }
        public bool IsMoving => _state == EClientState.Moving;
        public bool IsOrdering => _state == EClientState.Ordering;
        public bool IsSitting => _state == EClientState.Sitting;
        public float ReachDistance { get => _reachDistance; set => _reachDistance = value; }

        #endregion

        #region Fields
        private float _speed = 1f;
        private SplineContainer _splineComp;
        private List<Counter> _counters;

        private float _reachDistance = 2f;
        private Counter _currentCounter;

        // It's not a clean state machine for now. To be refactored later, probably. It's quite messy as is
        private EClientState _state = EClientState.Moving;
        private bool _hasOrdered = false;

        private ClientMovement _movement;
        private bool _debugSplinePosition;
        private Order _order;
        #endregion

        #region Methods

        private void Start()
        {
            _movement = new(transform, _speed, _splineComp);

            _counters = ReferenceManager.Instance.GetElementsOfType<Counter>();
            if (_counters == null || _counters.Count == 0)
                Debug.LogError("[Client] Start : No counters were found. Please place at least one of each in the scene.");

            if (_splineComp == null)
                Debug.LogError("[Client] Init : No spline component was referenced. Consider using SetSplinePath.");

            _movement.SetSplinePos(0.0f);
        }

        private void Update()
        {
            if (_state == EClientState.Moving)
            {
                _movement.MoveAlongSpline();
                if (!_hasOrdered)
                { 
                    if (CheckForCounter(out OrderingCounter counter))
                        UseCounter(counter);
                }
                else if (CheckForCounter(out SittingCounter counter))
                {
                    UseCounter(counter);
                }
            }
            else if (_state == EClientState.AtCounter)
            {
                _movement.GoTo(_currentCounter.UserSpot.position);
            }
            else if (_state == EClientState.ReachingSpline)
            {
                Vector3 nearestPos = _movement.FindNearestSplinePosition();
                _movement.GoTo(nearestPos);
                if (Vector3.Distance(transform.position, nearestPos) <= 0.03f)
                {
                    _state = EClientState.Moving;
                    _movement.SnapToSpline();
                }
            }
        }

        /// <summary>
        /// Detects if a free counter of type T is available nearby.
        /// </summary>
        /// <typeparam name="T">The specific counter type to look for</typeparam>
        /// <returns>The first free counter found</returns>
        private T CheckForCounter<T>() where T : Counter
        {
            foreach (var counter in _counters)
            {
                if (counter is not T) continue;
                if (Vector3.Distance(transform.position, counter.transform.position) < Mathf.Max(_reachDistance, counter.ReachDistance))
                {
                    if (!counter.IsFree) continue;
                    return counter as T;
                }
            }
            return null;
        }
        /// <summary>
        /// Detects if a free counter of type T is available nearby.
        /// </summary>
        /// <typeparam name="T">The specific counter type to look for</typeparam>
        /// <returns>True if a counter is found, false otherwise</returns>
        private bool CheckForCounter<T>(out T counter) where T : Counter
        {
            counter = CheckForCounter<T>();
            return counter != null;
        }

        private void UseCounter(Counter counter)
        {
            _state = EClientState.AtCounter;
            _currentCounter = counter;
            _currentCounter.Register(this);
        }

        public void AcceptOrder(Order order)
        {
            if (_state == EClientState.AtCounter && !_hasOrdered)
            {
                _hasOrdered = true;
                _state = EClientState.ReachingSpline;
                _order = order;
            }
        }

        public void ResolveOrder(SplineContainer newPath)
        {
            if (_state == EClientState.AtCounter && _hasOrdered)
            {
                _splineComp = newPath;
                _state = EClientState.ReachingSpline;
            }
        }

        public void Kill()
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