using GameSystem;
using System;
using UnityEngine;

namespace Features
{
    public class ClientSeat : Feature
    {

        public Action<Client> OnClientRegister {  get; set; }
        public Action<Client> OnClientUnregister {get; set; }
        public Client CurrentClient
        {
            get => _currentClient;
            set
            {
                if (_currentClient == value) return;
                if (_currentClient != null)
                    OnClientUnregister?.Invoke(_currentClient);
                if (value != null)
                    OnClientRegister?.Invoke(value);
                _currentClient = value;
            }
        }

        public Transform transform;

        private Client _currentClient;
        

        public ClientManager ClientManager => Reference.GetManager<ClientManager>();
        public ClientSeat(IFeatureCollection owner, Transform seatTransform) : base(owner)
        {
            if (seatTransform == null)
                Debug.LogWarning("ClientSeat transform is null");
            transform = seatTransform;

            ClientManager.Register(this);
        }

        public bool TryRegister(Client client)
        {
            if (client == null) return false;
            if (CurrentClient != null) return false;

            CurrentClient = client;
            return true;
        }
        public bool TryUnregister(Client client)
        {
            if (client == null) return false;
            if (CurrentClient != client) return false;

            CurrentClient = null;
            return true;
        }
    }
}