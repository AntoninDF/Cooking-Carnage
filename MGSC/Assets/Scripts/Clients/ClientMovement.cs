using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace Client
{

    public class ClientMovement
    {
        #region Properties

        public SplineContainer SplineComponent { get; set; }
        public Spline Spline => SplineComponent.Spline;
        public float SplinePosition { get; private set; }
        public Transform ControlledTransform { get; set;}
        public float Speed { get;set;}
        public List<Vector3> CheckedPositions { get; set; } = new();

        #endregion

        #region Fields


        #endregion

        #region Methods

        public ClientMovement(Transform controlledTransform, float speed, SplineContainer splineComponent)
        {
            ControlledTransform = controlledTransform;
            SplineComponent = splineComponent;
            Speed = speed;
        }

        public void MoveAlongSpline()
        {
            float length = Spline.GetLength();
            SetSplinePos(SplinePosition + (Speed * Time.deltaTime / length));
        }

        public void SetSplinePos(float t)
        {
            SplinePosition = t;
            ControlledTransform.position = GetSplinePos(t);
        }

        public Vector3 GetSplinePos(float t)
        {
            Spline.Evaluate(
                t,
                out Unity.Mathematics.float3 pos,
                out Unity.Mathematics.float3 tangent,
                out Unity.Mathematics.float3 upVector);
            return pos;
        }

        public void SnapToSpline()
        {
            SetSplinePos(FindNearestSplinePoint());
        }

        private float FindNearestSplinePoint()
        {
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

        public Vector3 FindNearestSplinePosition()
        {
            return GetSplinePos(FindNearestSplinePoint());
        }

        public void GoTo(Vector3 targetPos)
        {
            Vector3 startPos = ControlledTransform.position;
            Vector3 offset = targetPos - startPos;
            Vector3 finalPos;
            float targetDist = offset.magnitude;
            float maxDist = Time.deltaTime * Speed;
            float dist = Mathf.Min(targetDist, maxDist);
            finalPos = startPos + offset.normalized * dist;

            ControlledTransform.position = finalPos;
        }
        #endregion
    }

}