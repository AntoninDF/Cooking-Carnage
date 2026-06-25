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
        public Action OnWaitLineFull { get; set; }

        private Queue<Client>   _queue = new();
        private SplineContainer _spline;
        private float           _anchorTime = -1f; 
        private int   _maxQueueSize    = 1;


        public int Count => _queue.Count;

        public ClientWaitingQueue(IFeatureCollection owner, SplineContainer spline) : base(owner)
        {
            _spline = spline;
        }

        public float Enqueue(Client client)
        {
            _queue.Enqueue(client);
            if(IsQueueFull()) OnWaitLineFull?.Invoke();
            return GetWaitTime(_queue.Count - 1);
        }
        
        public bool Remove(Client client)
        {
            if (!_queue.Contains(client)) return false;

            var list = new List<Client>(_queue);
            list.Remove(client);
            _queue = new Queue<Client>(list);

            RefreshPositions();
            //Reference.GetManager<AdaptiveClientSpawner>()?.OnQueueSlotFreed();
            return true;
        }

        public void OnSeatFreed(ClientSeat freedSeat)
        {
            if (_queue.Count == 0) return;

            Client first = _queue.Dequeue();
            //first.Behaviour.WaitInQueue.StopWaiting();
            //freedSeat.TryRegister(first);

            RefreshPositions();
            OnSeatAvailable?.Invoke(first, freedSeat);
            
            //Reference.GetManager<AdaptiveClientSpawner>()?.OnQueueSlotFreed();
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
        
        public bool IsQueueFull()
        {
            return _queue.Count == _maxQueueSize;
        }
        
        
    }
}