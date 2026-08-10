using Clients;
using GameSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Features
{
    public class ClientSeat : Feature
    {
        // A client registers a seat before it actually sits on it.
        // Like saying "Hey, don'elapsed take this seat, it's mine"
        // Usually, a client unregisters at the same time they leave
        public Action<Client> OnClientRegister {  get; set; }
        public Action<Client> OnClientUnregister {get; set; }
        
        public Action<Client> OnClientSitting {get; set; }
        public Action OnClientLeaving {get; set; }
        public Action OnClientLeavingWithTips {get; set; }
        
        public Action OnClientKilled {get; set; }
        public Action OnMealReady { get; set; }
        public Action OnIngredientDropped { get; set; }
        public Action<Client> ClientLeaveTheSeat {get; set; }
        
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

            GameLoop.OnReset += Reset;
        }

        public override void Reset()
        {
            base.Reset();
            RegisteredClient = null;
            SittingClient = null;
            parentTransform.localPosition = _defaultPosition;
        }

        public bool TryRegister(Client client)
        {
            if (client == null)return false;
            if (RegisteredClient != null)return false;
            
            client.CurrentSeat        =  this;
            client.Behaviour.OnReactionEnd += OnClientLeaving;
            client.Behaviour.GoodReaction += OnClientLeavingWithTips;
            RegisteredClient          =  client;
            
            return true;
        }
        
        public bool TryUnregister(Client client)
        {
            if (client == null) return false;
            if (RegisteredClient != client) return false;
            
            if(!client.IsCooked)
            {
                TryLeave(client);
                if (client.IsAngry)
                    ActualizeSeatPos(null);
                else
                    TimerManager.StartTimer("Lower client seat (" + Time.time + ")", 5.5f, () => ActualizeSeatPos(null));
            }
            else
            {
                OnIngredientDropped?.Invoke();
                client.SpawnIngredients(dropSlots);
                ActualizeSeatPos(null);
            }
    
            //parentTransform.localPosition = _defaultPosition;
            var unregistered = _registeredClient;
            OnClientUnregister?.Invoke(unregistered);
            
            client.CurrentSeat  = null;
            client.OrderLog = client.CurrentOrder;
            client.CurrentOrder = null;

            
            RegisteredClient = null;
            SittingClient    = null;

            return true;
        }
        
        public bool TrySit(Client client)
        {
            if (client == null) return false;
            if (RegisteredClient != client) return false;

            SittingClient = client;
            
            client.SittingClient(transform);

            ActualizeSeatPos(client);

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
            //OnClientUnregister?.Invoke(client);

            //OnClientLeaving?.Invoke(client);
            return true;
        }

        private void ActualizeSeatPos(Client client)
        {
            float duration = 1.5f;
            Vector3 startPos = parentTransform.localPosition;
            TimerManager.StartGameplayTimer("Seat moving (" + Time.time + ")", duration, null, elapsed => LerpSeatPos(client, startPos, ClientSeatPosition(client), elapsed / duration));
            //(Owner as OrderingArea)?.StartCoroutine(LerpSeatPos(client, ClientSeatPosition(client)));
        }

        void LerpSeatPos(Client client, Vector3 startPos, Vector3 targetPos, float t)
        {
            var curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            parentTransform.localPosition = Vector3.Lerp(startPos, targetPos, curve.Evaluate(t));
        }

        public Vector3 ClientSeatPosition(Client client)
        {
            float y = 0;
            if (client != null)
                foreach (var heightByType in _servingAreaConfig.ClientHeightByTypeList)
                    if (heightByType.ClientType == client.Type)
                        y = heightByType.SeatHeight;
            
            return new Vector3(_defaultPosition.x, _defaultPosition.y + y, _defaultPosition.z);
        }
    }
}