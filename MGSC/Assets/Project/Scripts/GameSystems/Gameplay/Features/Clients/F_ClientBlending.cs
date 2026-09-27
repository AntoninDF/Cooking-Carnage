using System;
using Clients;
using GameSystem;
using UnityEngine;

namespace Features
{
    public class F_ClientBlending : Feature
    {
        public Action  OnCustomerBlended { get; set; }

        private float  _destroyDelay;
        private float  _timer;
        private bool   _isRunning;
        private Client _client;
        
        public F_ClientBlending(IFeatureCollection owner, float destroyDelay = 2f) : base(owner)
        {
            _client       = owner as Client;
            _destroyDelay = destroyDelay;
        }
        
        public void Trigger()
        {
            if (!Globals.Authorizations.AllowMakingDrinks) return;
            if (_isRunning) return;
            _isRunning       = true;
            _timer           = 0f;
            _client.IsBlended = true;
            Reference.GetManager<OrderManager>().CancelOrder(_client.CurrentOrder);
            OnCustomerBlended?.Invoke();
            _client.CurrentSeat?.TryUnregister(_client);

        }

        public override void Update()
        {
            if (!_isRunning) return;

            _timer += Time.deltaTime;
            if (_timer >= _destroyDelay)
                BlendClient();
        }

        private void BlendClient()
        {
            _isRunning = false;

            if (Reference.TryGetManager(out ClientManager cm))
            {
                cm.WaitingQueue?.Remove(_client);
            }

            var queue = cm?.WaitingQueue;
            if (queue != null) queue.OnSeatAvailable -= _client.Behaviour.OnQueueSeatAvailablePublic;

            _client.Despawn();
        }
    }
}