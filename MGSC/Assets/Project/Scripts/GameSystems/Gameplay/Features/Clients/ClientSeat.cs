using GameSystem;
using System;
using Clients;
using UnityEngine;

namespace Features
{
    public class ClientSeat : Feature
    {
        // A client registers a seat before it actually sits on it.
        // Like saying "Hey, don't take this seat, it's mine"
        // Usually, a client unregisters at the same time they leave
        public Action<Client> OnClientRegister {  get; set; }
        public Action<Client> OnClientUnregister {get; set; }
        public Action<Client> OnClientSitting {get; set; }
        public Action<Client> OnClientLeaving {get; set; }
        public Client RegisteredClient
        {
            get => _registeredClient;
            set
            {
                if (_registeredClient == value) return;
                if (_registeredClient != null)
                    OnClientUnregister?.Invoke(_registeredClient);
                if (value != null)
                    OnClientRegister?.Invoke(value);
                _registeredClient = value;
                //Debug.Log("Registered client is " + value);
            }
        }
        public Client SittingClient
        {
            get => _sittingClient;
            set
            {
                if (_sittingClient == value) return;
                if (_sittingClient != null)
                    OnClientLeaving?.Invoke(_sittingClient);
                if (value != null)
                    OnClientSitting?.Invoke(value);
                _sittingClient = value;
                //Debug.Log("Sitting client is " + value);
            }
        }

        public Transform transform;

        private Client _registeredClient;
        private Client _sittingClient;
        

        public ClientManager ClientManager => Reference.GetManager<ClientManager>();
        public ClientSeat(IFeatureCollection owner, Transform seatTransform) : base(owner)
        {
            /*
            if (seatTransform == null)
                Debug.LogWarning("ClientSeat transform is null");
            */
            transform = seatTransform;

            ClientManager.Register(this);
        }

        public bool TryRegister(Client client)
        {
            if (client == null) return false;
            if (RegisteredClient != null) return false;

            client.CurrentSeat = this;
            RegisteredClient = client;
            return true;
        }
        public bool TryUnregister(Client client)
        {
            //Debug.Log("Trying to unregister...");
            if (client == null) return false;
            if (RegisteredClient != client) return false;

            RegisteredClient = null;
            SittingClient = null;
            //Debug.Log("Unregistered !");
            return true;
        }
        public bool TrySit(Client client)
        {
            if (client == null) return false;
            if (RegisteredClient != client) return false;

            SittingClient = client;
            return true;
        }
        public bool TryLeave(Client client)
        {
            if (client == null) return false;
            if (SittingClient != client) return false;

            SittingClient = null;
            return true;
        }
    }
}