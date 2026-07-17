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
        #region Properties

        public int Count => _queue.Count;
        public float SpacingTime { get; set; } = 0.075f;
        public float QueueStartT { get; set; } = 0.3f;
        public Action<Client, ClientSeat> OnSeatAvailable { get; set; }
        public Action OnWaitLineFull { get; set; }

        #endregion

        #region Fields

        private Queue<Client>   _queue = new();
        private int   _maxQueueSize    = 1;

        #endregion



        public ClientWaitingQueue(IFeatureCollection owner) : base(owner)
        {
            GameLoop.OnReset += Clear;
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
        }

        /// <summary>
        /// Actualize the positions of waiting clients when list is updated
        /// </summary>
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
        /// To kown if the wait line is full
        /// </summary>
        /// <returns></returns>
        public bool IsQueueFull()
        {
            return _queue.Count >= _maxQueueSize;
        }
        
        public void Clear() 
        {
            _queue.Clear();
        }
        
    }
}