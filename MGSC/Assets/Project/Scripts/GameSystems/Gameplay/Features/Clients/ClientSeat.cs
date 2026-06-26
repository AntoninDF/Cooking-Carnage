using GameSystem;
using System;
using Clients;
using Project.Scripts.GameSystems.Gameplay.Components.Clients;
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
        
        public Action OnMealReady { get; set; }
        
        public Client RegisteredClient
        {
            get => _registeredClient;
            set
            {
                if (_registeredClient == value) return;
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

        private Client  _registeredClient;
        private Client  _sittingClient;
        private Vector3 _defaultPosition;
        

        public ClientManager ClientManager => Reference.GetManager<ClientManager>();
        public ClientSeat(IFeatureCollection owner, Transform seatTransform) : base(owner)
        {
            /*
            if (seatTransform == null)
                Debug.LogWarning("ClientSeat transform is null");
            */
            transform        = seatTransform;
            _defaultPosition = seatTransform.position;
            ClientManager.Register(this);
        }

        public bool TryRegister(Client client)
        {
            if (client == null)
            {
                //Debug.Log("No client found");
                return false;
            }
            if (RegisteredClient != null)
            {
                //Debug.Log($"A client is already  registered : {RegisteredClient.name}");
                return false;
            }
            
            //Debug.Log($"Registering client : {client.name}");
            client.CurrentSeat = this;
            RegisteredClient = client;
            ActualizeSeatPos(client);
            return true;
        }
        public bool TryUnregister(Client client)
        {
            //Debug.Log("Trying to unregister...");
            if (client == null) return false;
            if (RegisteredClient != client) return false;

            RegisteredClient = null;
            SittingClient = null;
            //Debug.Log($"Unregistered : {client.name}!");
            OnClientUnregister?.Invoke(_registeredClient);
            return true;
        }
        public bool TrySit(Client client)
        {
            if (client == null)
            {
                Debug.Log("Client null");
                return false;
            }

            if (RegisteredClient != client)
            {
                Debug.Log("Registered Client is not the sitting one");
                return false;
            }

            SittingClient = client;
            client.transform.SetParent(transform);
            client.Behaviour.ClientMovement.Sit();
            client.ClientLook.Sit();
            Debug.Log($"Seit : {client.name}");
            client.Behaviour.OnSitting?.Invoke();
            return true;
        }
        public bool TryLeave(Client client)
        {
            if (client == null) return false;
            if (SittingClient != client) return false;

            SittingClient = null;
            client.transform.SetParent(null);
            client.Behaviour.ClientMovement.GetUp();
            return true;
        }

        private void ActualizeSeatPos(Client client)
        {
            transform.position = ClientSeatPosition(client);
        }

        public Vector3 ClientSeatPosition(Client client)
        {
            float y = 0;
            foreach (var heightByType in SO_ClientSeatHeight.instance.ClientHeightByTypes)
            {
                if (heightByType.ClientType == client.Type) y = heightByType.SeatHeight;
            }

            return new Vector3(_defaultPosition.x, _defaultPosition.y + y, _defaultPosition.z);
        }
    }
}