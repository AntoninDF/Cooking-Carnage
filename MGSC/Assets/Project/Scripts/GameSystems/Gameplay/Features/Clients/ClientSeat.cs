using GameSystem;
using System;
using System.Collections.Generic;
using Clients;
using Cooking;
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
            }
        }

        public Transform transform;
        public Transform parentTransform;
        public  List<SlotContainer> dropSlots = new List<SlotContainer>();

        private Client  _registeredClient;
        private Client  _sittingClient;
        private Vector3 _defaultPosition;
        private ClientSeatHeightConfig _servingAreaConfig;

        public ClientManager ClientManager => Reference.GetManager<ClientManager>();
        public ClientSeat(IFeatureCollection owner, Transform seatTransform, Transform seatParent, ClientSeatHeightConfig servingAreaConfig) : base(owner)
        {
            transform        = seatTransform;
            parentTransform = seatParent;
            _defaultPosition = parentTransform.localPosition;
            ClientManager.Register(this);
            _servingAreaConfig = servingAreaConfig;
        }

        public bool TryRegister(Client client)
        {
            if (client == null)return false;
            if (RegisteredClient != null)return false;
            
            client.CurrentSeat = this;
            RegisteredClient = client;
            ActualizeSeatPos(client);
            return true;
        }
        
        public bool TryUnregister(Client client)
        {
            if (client == null) return false;
            if (RegisteredClient != client) return false;
            
            if(!client.IsCooked)
            {
                TryLeave(client);
            }
            else
            {
                client.SpawnIngredients(dropSlots);
            }
    
            parentTransform.localPosition = _defaultPosition;

            client.CurrentSeat  = null;
            client.CurrentOrder = null;

            var unregistered = _registeredClient;
            RegisteredClient = null;
            SittingClient    = null;
            OnClientUnregister?.Invoke(unregistered);
            return true;
        }
        
        public bool TrySit(Client client)
        {
            if (client == null) return false;
            if (RegisteredClient != client) return false;

            SittingClient = client;
            
            client.SittingClient(transform);
            
            return true;
        }
        
        public bool TryLeave(Client client)
        {
            if (client == null) return false;
            if (SittingClient != client)
            {
                return false;
            }
            SittingClient = null;
            return true;
        }

        private void ActualizeSeatPos(Client client)
        {
            parentTransform.localPosition = ClientSeatPosition(client);
        }

        public Vector3 ClientSeatPosition(Client client)
        {
            float y = 0;
            foreach (var heightByType in _servingAreaConfig.ClientHeightByTypeList)
            {
                if (heightByType.ClientType == client.Type)
                {
                    y = heightByType.SeatHeight;
                }
            }
            
            return new Vector3(_defaultPosition.x, _defaultPosition.y + y, _defaultPosition.z);
        }
    }
}