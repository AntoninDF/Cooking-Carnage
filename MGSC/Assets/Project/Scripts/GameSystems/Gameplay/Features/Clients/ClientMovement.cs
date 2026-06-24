using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;

namespace Features
{
    public class ClientMovement : Feature
    {
        #region Properties
        
        
        public float Speed { get;set;}
        public float SplinePosition { get; private set; }
        
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
        public Action OnDestinationReached { get; set; }
        public Action OnStartMoving { get; set; }
        public Action OnStopMoving { get; set; }
    
        public Transform ControlledTransform { get; set;}
        public SplineContainer SplineComponent { get; set; }
        public List<Vector3> CheckedPositions { get; set; } = new();
        public Spline Spline => SplineComponent?.Spline;

        
        #endregion

       

        #region Fields

        NavMeshAgent _agent { get; set; } 
        private bool _validSetup;
        private bool _isMoving;

        #endregion
    
    
        #region Methods
        public ClientMovement(IFeatureCollection owner,Transform controlledTransform, NavMeshAgent agent,  SplineContainer splineComponent, float speed) : base(owner)
        {
            _agent = agent;
            ControlledTransform = controlledTransform;
            SplineComponent = splineComponent;
            Speed = speed;
            _agent.speed = Speed;
            if (ControlledTransform != null && SplineComponent != null)
            {
                
                _validSetup = true;
            }
            else
            {
                Debug.Log($"The null component is ControlledTransform : {ControlledTransform == null} or SplineComponent : {SplineComponent == null}");
                Debug.LogWarning("[ClientMovement] : The client's movement was not initialized properly");
            }
        }
        
        
        public void SetDestination(Vector3 worldPosition)
        {
            _agent.SetDestination(worldPosition);
            IsMoving = true;
        }
        public void SetDestination(ClientSeat seat)
        {
            if (seat == null)
            {
                Debug.LogWarning("ClientMovement.SetDestination : Seat is null");
                return;
            }
            _agent.SetDestination(seat.transform.position);
        }
        public override void Update()
        {
            HasReachedDestination();
        }
    
        public bool HasReachedDestination()
        {
            // https://discussions.unity.com/t/how-can-i-tell-when-a-navmeshagent-has-reached-its-destination/52403/5
            // Check if we've reached the destination
            if (!_agent.pathPending)
            {
                if (_agent.remainingDistance <= _agent.stoppingDistance)
                {
                    if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)
                    {
                        if (IsMoving) OnDestinationReached?.Invoke();
                        IsMoving = false;
                        return true;
                    }
                }
            }
            return false;
        } 
        
        public void MoveAlongSpline()
        {
            if (!_validSetup)
            {
                return;
            }
            float length = Spline.GetLength();
            SetSplinePos(SplinePosition + (Speed * Time.deltaTime / length));
            
        }
        
        public void SetSplinePos(float t)
        {
            if (!_validSetup)
            {
                return;
            }
            SplinePosition = t;
            ControlledTransform.position = GetSplinePos(t);
        }
        
        public Vector3 GetSplinePos(float t)
        {
            if (!_validSetup)
            {
                return Vector3.zero;
            }

            Spline.Evaluate(
                t,
                out Unity.Mathematics.float3 pos,
                out Unity.Mathematics.float3 tangent,
                out Unity.Mathematics.float3 upVector);
            return (Vector3)pos + SplineComponent.transform.position;
        }
        
        public float FindNearestSplineT(Vector3 worldPosition)
        {
            Vector3 saved = ControlledTransform.position;
            ControlledTransform.position = worldPosition;
            float t = FindNearestSplinePoint();
            ControlledTransform.position = saved;
            return t;
        }
        
        private float FindNearestSplinePoint()
        {
            if (!_validSetup)
            {
                return 0.0f;
            }
            CheckedPositions.Clear();
            float t = 0;
            float errorMargin = 0.001f;
            int maxChecks = 40;

            float lastDistance = float.MaxValue;

            float distanceBetweenInitialChecks = 1f;
            int initialChecks = Mathf.CeilToInt(Spline.GetLength() / distanceBetweenInitialChecks);
            initialChecks = Mathf.Min(maxChecks/2, initialChecks);

            for (int i = 0; i <= initialChecks; i++)
            {
                float dist = Vector3.Distance(ControlledTransform.position, GetSplinePos(1f/initialChecks * i));
                if (dist < lastDistance)
                {
                    lastDistance = dist;
                    t = 1f / initialChecks * i;
                }
                CheckedPositions.Add(GetSplinePos(1f / initialChecks * i));
            }


            float offset = 0.5f/initialChecks;
            for (int i = 0; i < maxChecks/2; i++)
            {
                float currentDistance = Vector3.Distance(ControlledTransform.position, GetSplinePos(t));
                CheckedPositions.Add(GetSplinePos(t));
                if (currentDistance > lastDistance)
                {
                    offset *= -0.5f;
                }
                lastDistance = currentDistance;

                if (Mathf.Abs(offset) <= errorMargin)
                {
                    return t;
                }

                t += offset;
                t = Mathf.Clamp(t, 0, 1);
            }

            return t;
        }
        
        #endregion
    }
}