using System;
using System.Collections.Generic;
using Clients;
using GameSystem;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Features
{
    /// <summary>
    /// The feature in charge of the Waiting Behavior
    /// </summary>
	public class ClientWaitingQueue : Feature
	{
        /// <summary>
        /// The ecart betwen two customers in the waiting Line (in time)
        /// </summary>
        public float SpacingTime { get; set; } = 0.075f;
        
        /// <summary>
        /// the starting point of the waiting line in the spline (in time)
        /// </summary>
        public float QueueStartT { get;                          set; } = 0.3f;
        public Action<Client, ClientSeat> OnSeatAvailable { get; set; }


        private Queue<Client>   _queue = new();
        private SplineContainer _spline;
        private float           _anchorTime = -1f; 

        public int Count => _queue.Count;

        public ClientWaitingQueue(IFeatureCollection owner, SplineContainer spline) : base(owner)
        {
            _spline = spline;
        }

        public float Enqueue(Client client)
        {
            _queue.Enqueue(client);
            return GetWaitTime(_queue.Count - 1);
        }

        public void OnSeatFreed(ClientSeat freedSeat)
        {
            if (_queue.Count == 0) return;

            Client first = _queue.Dequeue();
            freedSeat.TryRegister(first);

            RefreshPositions();
            OnSeatAvailable?.Invoke(first, freedSeat);

            Reference.GetManager<AdaptiveClientSpawner>()?.OnQueueSlotFreed();
        }

        private void RefreshPositions()
        {
            int index = 0;
            foreach (var client in _queue)
            {
                float waitT = GetWaitTime(index);
                client.Behaviour.SetWaitPosition(waitT);
                index++;
            }
        }

        /// <summary>
        /// Convert the wanted wait position in curve t parameter
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private float GetWaitTime(int index)
        {
            return Mathf.Clamp(QueueStartT - (index + 1) * SpacingTime, 0f, 1f);
        }

        /// <summary>
        /// Find the closest point to the nearest seat of the begining of the curve (t = 0 point)
        /// </summary>
        /// <returns></returns>
        private float ComputeAnchorTime()
        {
            var seats = Reference.GetManager<ClientManager>().GetClientSeats(_ => true, mustBeAvailable: false);

            float closestT = 1f;

            foreach (var seat in seats)
            {
                if (seat?.transform == null) continue;
                float t = FindNearestTime(seat.transform.position);
                if (t < closestT)
                    closestT = t;
            }

            return closestT;
        }

        /// <summary>
        /// Give the closest point of the curve to a world position
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        private float FindNearestTime(Vector3 worldPosition)
        {
            if (_spline == null) return 0f;
            var spline = _spline.Spline;
            float bestT = 0f;
            float bestDist = float.MaxValue;
            int steps = 40;

            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                spline.Evaluate(t, out var pos, out _, out _);
                float dist = Vector3.Distance((Vector3)pos + _spline.transform.position, worldPosition);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestT = t;
                }
            }
            return bestT;
        }

        public Vector3 GetStartPosition()
        {
            _spline.Spline.Evaluate(QueueStartT, out var pos, out _, out _);
            return pos;
        }
    }
}