using UnityEngine;
using Cooking;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Splines;
using System;

namespace Client
{

    public class Client : MonoBehaviour
    {
        #region Properties
        public EClientType Type { get; private set; }
        public SO_Recipe Order {  get; private set; }
        public List<SO_Ingredient> IngredientsToDrop { get; private set; }
        public SO_Client Model { get; private set; }
        public float Speed { get => _speed; set => _speed = value; }
        public bool IsMoving => _state == EClientState.Moving;
        public bool IsOrdering => _state == EClientState.Ordering;
        public bool IsSitting => _state == EClientState.Sitting;
        public float ReachDistance { get => _reachDistance; set => _reachDistance = value; }

        #endregion

        #region Fields
        private float _speed;
        private SplineContainer _splineComp;
        private List<OrderingCounter> _orderingCounters;
        private List<SittingCounter> _sittingCounters;

        private float _splinePosition;
        private float _reachDistance = 2f;
        private OrderingCounter _currentOrderingCounter;
        private SittingCounter _currentSeat;

        // It's not a clean state machine for now. To be refactored later, probably. It's quite messy as is
        private EClientState _state = EClientState.Moving;
        private bool _hasOrdered = false;
        private Vector3 _nearestPos;
        #endregion

        #region Methods

        private void Start()
        {
            _orderingCounters = ReferenceManager.Instance.GetElementsOfType<OrderingCounter>();
            if (_orderingCounters == null || _orderingCounters.Count == 0)
                Debug.LogError("[Client] Start : No ordering counter was found. Please place at least one in the scene.");

            _sittingCounters = ReferenceManager.Instance.GetElementsOfType<SittingCounter>();
            if (_orderingCounters == null || _orderingCounters.Count == 0)
                Debug.LogError("[Client] Start : No sitting counter was found. Please place at least one in the scene.");
        }

        private void Update()
        {
            if (_state == EClientState.Moving)
            {
                Move();
                if (!_hasOrdered)
                    CheckForOrderingArea();
                else
                    CheckForSittingArea();
            }
            else if (_state == EClientState.Ordering)
            {
                GoTo(_currentOrderingCounter.OrderingSpot.position);
            }
            else if (_state == EClientState.Sitting)
            {
                GoTo(_currentSeat.SittingSpot.position);
            }
            else if (_state == EClientState.ReachingSpline)
            {
                GoTo(_nearestPos);
                if (Vector3.Distance(transform.position, _nearestPos) <= 0.3f)
                {
                    _state = EClientState.Moving;
                }
            }
        }

        private bool CheckForSittingArea()
        {
            foreach (var seat in _sittingCounters)
            {
                if (Vector3.Distance(transform.position, seat.transform.position) < Mathf.Max(_reachDistance, seat.ReachDistance))
                {
                    if (!seat.IsFree) continue;
                    _state = EClientState.Sitting;
                    _currentSeat = seat;
                    _currentSeat.Register(this);
                    return true;
                }
            }
            return false;
        }

        public void AcceptOrder()
        {
            if (_state == EClientState.Ordering && !_hasOrdered)
            {
                _hasOrdered = true;
                _state = EClientState.ReachingSpline;
                _nearestPos = GetSplinePos(FindNearestSplinePosition());
            }
        }

        private void GoTo(Vector3 position)
        {
            Vector3 startPos = GetSplinePos(_splinePosition);
            Vector3 endPos = position;
            Vector3 offset = endPos - startPos;
            float progression = (transform.position - startPos).magnitude;
            Vector3 targetPos;
            if (progression < offset.magnitude)
                targetPos = startPos + offset.normalized * progression + offset.normalized * _speed * Time.deltaTime;
            else
                targetPos = endPos;
            transform.position = targetPos;
        }

        public void Init()
        {
            if (_splineComp == null)
            {
                Debug.LogError("[Client] Init : No spline component was referenced. Consider using SetSplinePath.");
                return;
            }
            SetSplinePos(0.0f);
            this.SetSpeed(0.8f);
        }

        private void Move()
        {
            float length = _splineComp.Spline.GetLength();
            SetSplinePos(_splinePosition + (_speed * Time.deltaTime / length));
        }

        private void SetSplinePos(float t)
        {
            _splinePosition = t;
            transform.position = GetSplinePos(t);
        }

        private Vector3 GetSplinePos(float t)
        {
            _splineComp.Spline.Evaluate(
                t,
                out Unity.Mathematics.float3 startPos,
                out Unity.Mathematics.float3 tangent,
                out Unity.Mathematics.float3 upVector);
            return startPos;
        }

        private bool CheckForOrderingArea()
        {
            foreach(var counter in _orderingCounters)
            {
                if (Vector3.Distance(transform.position, counter.transform.position) < Mathf.Max(_reachDistance, counter.ReachDistance))
                {
                    _state = EClientState.Ordering;
                    _currentOrderingCounter = counter;
                    _currentOrderingCounter.Register(this);
                    return true;
                }
            }
            return false;
        }

        private float FindNearestSplinePosition()
        {
            float t = _splinePosition;
            float errorMargin = 0.005f;
            int maxChecks = 100;

            float offset = 0.3f;
            float lastDistance = float.MaxValue;
            for (int i = 0; i < maxChecks; i++)
            {
                Debug.Log("Offset : " + offset);
                float currentDistance = Vector3.Distance(transform.position, GetSplinePos(t));
                if (currentDistance > lastDistance)
                {
                    offset *= -0.5f;
                }
                lastDistance = currentDistance;

                if (Mathf.Abs(offset) <= errorMargin)
                    return t;

                t += offset;
                t = Mathf.Clamp(t, 0, 1);
            }


            return t;
        }

        #region Client creation
        /// <summary>
        /// This is the advised way to spawn a new client
        /// </summary>
        /// <param name="model">The client data model</param>
        /// <param name="order">What the client will order at the counter</param>
        /// <param name="position">The position at which the gameobject will be instantiated</param>
        /// <returns></returns>
        public static Client New(SO_Client model, SO_Recipe order, Vector3 position = default(Vector3))
        {
            if (model == null) {
                Debug.LogError("[Client] New : The specified client model was null. Please pass in a valid model");
                return null;
            }
            return 
                Client.New(model.prefab, model.clientType, model.loot, order, model.name, position)
                .SetModel(model);
        }
        /// <summary>
        /// If you spawn a client this way, you should probably use SetModel on it afterwards.
        /// </summary>
        /// <param name="prefab">The GameObject that will be spawned. It should be of the appropriate species</param>
        /// <param name="type">The client type</param>
        /// <param name="ingredientsDroppedOnKill">List of all the ingredients dropped on kill. It might be modified based on gameplay situations</param>
        /// <param name="order">What the client will order at the counter</param>
        /// <param name="name">The name that will be used for the gameobject</param>
        /// <param name="position">The position at which the gameobject will be instantiated</param>
        /// <returns></returns>
        public static Client New(GameObject prefab, EClientType type, List<SO_Ingredient> ingredientsDroppedOnKill, SO_Recipe order, string name=null, Vector3 position=default(Vector3))
        {
            if (prefab == null)
            {
                Debug.LogError("[Client] New : The argument 'prefab' given to Client.New() was null");
                return null;
            }

            GameObject go = GameObject.Instantiate(prefab, position, Quaternion.identity);
            Client client = go.GetComponent<Client>();

            if (client == null)
            {
                Debug.LogError("[Client] New : The given prefab '" + prefab.name + "' has no Client component. Please add one to the root object");
                return null;
            }

            // Setup the client
            client
                .SetType(type)
                .SetOrder(order)
                .SetLoot(ingredientsDroppedOnKill);

            if (name != null)
                go.name = name;

            return client;
        }
        #endregion
        #region Client properties
        public Client SetType(EClientType type)
        {
            Type = type;
            return this;
        }

        public Client SetOrder(SO_Recipe order)
        {
            Order = order;
            return this;
        }

        public Client SetLoot(List<SO_Ingredient> loot)
        {
            IngredientsToDrop = loot;
            return this;
        }

        public Client SetModel(SO_Client model)
        {
            Model = model;
            return this;
        }

        public Client SetSplinePath(SplineContainer splineObject)
        {
            _splineComp = splineObject;
            return this;
        }

        public Client SetSpeed(float speed)
        {
            _speed = speed;
            return this;
        }

        public Client SetReachDistance(float distance)
        {
            _reachDistance = distance;
            return this;
        }
        #endregion

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Debug.Log("Drawing gizmos");
            Gizmos.DrawSphere(GetSplinePos(FindNearestSplinePosition()), 0.1f);
        }
        #endregion

        private enum EClientState
        {
            None = 0,
            Moving = 1,
            Ordering = 2,
            Sitting = 3,
            ReachingSpline = 4
        }


    }
    
}