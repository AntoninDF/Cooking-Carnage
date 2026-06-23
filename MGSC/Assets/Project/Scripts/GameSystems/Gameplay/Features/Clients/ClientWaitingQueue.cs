using System.Collections.Generic;
using Clients;
using GameSystem;
using UnityEngine;
using UnityEngine.Splines;

namespace Features
{
	public class ClientWaitingQueue : Feature
	{
        public float SpacingT { get; set; } = 0.075f;

        private Queue<Client>   _queue = new();
        private SplineContainer _spline;
        private float           _anchorT = -1f; 

        public int Count => _queue.Count;

        public ClientWaitingQueue(IFeatureCollection owner, SplineContainer spline) : base(owner)
        {
            _spline = spline;
        }

        public float Enqueue(Client client)
        {
            _queue.Enqueue(client);
            return GetWaitT(_queue.Count - 1);
        }

        public void OnSeatAvailable(ClientSeat freedSeat)
        {
            if (_queue.Count == 0) return;

            Client first = _queue.Dequeue();
            freedSeat.TryRegister(first);
            first.AssignSeat(freedSeat);

            RefreshPositions();

            Reference.GetManager<AdaptiveClientSpawner>()?.OnQueueSlotFreed();
        }

        private void RefreshPositions()
        {
            int index = 0;
            foreach (var client in _queue)
            {
                float waitT = GetWaitT(index);
                client.SetWaitT(waitT);
                index++;
            }
        }

        private float GetWaitT(int index)
        {
            if (_anchorT < 0f)
                _anchorT = ComputeAnchorT();

            return Mathf.Clamp(_anchorT - (index + 1) * SpacingT, 0f, 1f);
        }

        private float ComputeAnchorT()
        {
            var seats = Reference.GetManager<ClientManager>()
                                 .GetClientSeats(_ => true, mustBeAvailable: false);

            float closestT = 1f;

            foreach (var seat in seats)
            {
                if (seat?.transform == null) continue;
                float t = FindNearestT(seat.transform.position);
                if (t < closestT)
                    closestT = t;
            }

            return closestT;
        }

        private float FindNearestT(Vector3 worldPosition)
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
    }
}