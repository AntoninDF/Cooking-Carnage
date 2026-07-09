using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

namespace Features
{
    public class ClientMovement : Feature
    {
        #region Properties
        
        public Transform ControlledTransform { get; set;}
        public SplineContainer SplineComponent { get; set; }
        public Spline Spline => SplineComponent?.Spline;
        
        public Action OnDestinationReached { get; set; }
        public Action OnStartMoving { get; set; }
        public Action OnStopMoving { get; set; }
        
        public Vector3 AgentVelocity => _navmeshAgent.enabled ? _navmeshAgent.velocity : Vector3.zero;
        
        public float Speed          { get; set;}
        public float SplinePosition { get; private set; }

        public bool  IsOnSpline     { get; private set; }
        public bool IsMoving
        {
            get => _isMoving;
            set
            {
                if (value == _isMoving) return;
                _isMoving = value;
                if (_isMoving) OnStartMoving?.Invoke();
                else OnStopMoving?.Invoke();
            }
        }
        
        #endregion
        
        #region Fields

        private List<Vector3> _checkedPositions { get; set; } = new();
        private NavMeshAgent  _navmeshAgent            { get; set; }
        private bool          _validSetup;
        private bool          _isMoving;

        #endregion

        #region setup
        /// <summary>
        /// Initialization of the Movement feature for the client
        /// assign all the required parameters
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="controlledTransform"></param>
        /// <param name="navmeshAgent"></param>
        /// <param name="splineComponent"></param>
        /// <param name="speed"></param>
        public ClientMovement(IFeatureCollection owner,Transform controlledTransform, NavMeshAgent navmeshAgent,  SplineContainer splineComponent, float speed) : base(owner)
        {
            _navmeshAgent              = navmeshAgent;
            ControlledTransform = controlledTransform;
            SplineComponent     = splineComponent;
            Speed               = speed;
            _navmeshAgent.speed        = speed;
            _navmeshAgent.stoppingDistance = 0.5f;
            //_navmeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            _navmeshAgent.avoidancePriority = Random.Range(0, 99);
            _validSetup         = controlledTransform != null && splineComponent != null;
            
           
        }

        #endregion

        #region Unity Lifecycle

        public override void Update()
        {
            if (!IsOnSpline) CheckNavDestination();
        }

        #endregion
        
        #region Spline Methods

        /// <summary>
        /// Calculate the next position of the custommer on the spline
        /// </summary>
        public void MoveAlongSpline()
        {
            if (!_validSetup)
            {
                return;
            }
            float lenght = Spline.GetLength();
            SetSplinePos(SplinePosition + Speed * Time.deltaTime/lenght);
        }
        
        /// <summary>
        /// Apply the calculated next position of the customers on the spline
        /// </summary>
        /// <param name="t"></param>
        public void SetSplinePos(float t)
        {
            if (!_validSetup)
            {
                return;
            }
            SplinePosition               = t;
            ControlledTransform.position = GetSplinePos(t);
        }
        
        /// <summary>
        /// Evaluate the spline position of t (time) parameter
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public Vector3 GetSplinePos(float time)
        {
            if (!_validSetup)
            {
                return Vector3.zero;
            }

            Spline.Evaluate(time, out Unity.Mathematics.float3 pos, out _, out _);
            return (Vector3)pos + SplineComponent.transform.position;
        }
        
        /// <summary>
        /// Return the t value of a position in the spline
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public float FindNearestSplineT(Vector3 worldPosition)
        {
            if (!_validSetup)
            {
                return 0f;
            }

            Vector3 saved = ControlledTransform.position;
            ControlledTransform.position = worldPosition;
            float t = FindNearestSplinePoint();
            ControlledTransform.position = saved;
            return t;
        }
        
        #endregion
        
        #region NavMesh Methods

        /// <summary>
        /// Switch the movement mode to navMesh
        /// The customer is moving toward the worldPosition parameter
        /// OnDestinationReached is fired once it reach destination
        /// </summary>
        /// <param name="worldPosition"></param>
        public void SwitchToNavMesh(Vector3 worldPosition)
        {
            IsOnSpline            = false;
            _navmeshAgent.enabled = true;
    
            bool warped = _navmeshAgent.Warp(ControlledTransform.position);
    
            SetNavDestination(worldPosition);
        }

        /// <summary>
        /// Disable the NavigationMesh and move alog the spline
        /// </summary>
        /// <param name="startTime"></param>
        public void SwitchToSpline(float startTime)
        {
            IsOnSpline     = true;
            _navmeshAgent.enabled = false;
            SetSplinePos(startTime);
        }

        /// <summary>
        /// Setup the target of the NavMesh agent
        /// </summary>
        /// <param name="worldPosition"></param>
        private bool _destinationSetThisFrame = false;

        public void SetNavDestination(Vector3 worldPosition)
        {
            if (!_navmeshAgent.enabled) return;
            _navmeshAgent.SetDestination(worldPosition);
            IsMoving                 = true;
            _destinationSetThisFrame = true;
        }

        /// <summary>
        /// Check if the NavMesh agent have reach the destination
        /// </summary>
        private void CheckNavDestination()
        {
            if (!_navmeshAgent.enabled || !IsMoving) return;
    
            if (_destinationSetThisFrame)
            {
                _destinationSetThisFrame = false;
                return; 
            }
    
            if (_navmeshAgent.pathPending) return;

            if (_navmeshAgent.remainingDistance <= _navmeshAgent.stoppingDistance &&
                (!_navmeshAgent.hasPath || _navmeshAgent.velocity.sqrMagnitude == 0f))
            {
                IsMoving = false;
                var callback = OnDestinationReached;
                OnDestinationReached = null;
                callback?.Invoke();
            }
        }
        #endregion

        #region Privates Methods

        /// <summary>
        /// Find the closest spline point to the customer
        /// Return the time value of this point
        /// </summary>
        /// <returns></returns>
        private float FindNearestSplinePoint()
        {
            _checkedPositions.Clear();
            float errorMargin = 0.001f;
            int   maxChecks    = 40;

            float distanceBetweenInitialsChecks = 1f;
            int initialChecks = Mathf.Min(maxChecks / 2,
                                          Mathf.CeilToInt(Spline.GetLength() / distanceBetweenInitialsChecks));
            float lastDistance = float.MaxValue;
            float t            = 0;
            
            for (int i = 0; i <= initialChecks; i++)
            {
                float sampleT = 1f / initialChecks * i;
                float dist    = Vector3.Distance(ControlledTransform.position, GetSplinePos(sampleT));
                if (dist < lastDistance)
                {
                    lastDistance = dist;
                    t            = sampleT;
                }
            }

            float offset = 0.5f/initialChecks;
            for (int i = 0; i < maxChecks; i++)
            {
                float currentDist = Vector3.Distance(ControlledTransform.position, GetSplinePos(t));
                if (currentDist > lastDistance)
                    offset *= -0.5f;
                lastDistance = currentDist;
                if (Mathf.Abs(offset) <= errorMargin) return t;
                t = Mathf.Clamp(t + offset, 0, 1f);
            }

            return t;
        }

        #endregion

        #region Public Methods

        public void Sit()
        {
            _navmeshAgent.enabled             = false; 
        }
        
        public void GetUp()
        {
            _navmeshAgent.enabled             = true;
        }

        #endregion
        
        
        
       
        
        
        
       
        
        
    }
}