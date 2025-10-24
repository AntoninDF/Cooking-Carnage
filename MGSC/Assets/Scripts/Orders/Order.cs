using UnityEngine;
using Clients;
using Cooking;
using UnityEngine.Events;
using System;

namespace Orders
{
    public class Order
    {
        #region Properties

        public Client Owner => _owner;
        public SO_Recipe Recipe => _recipe;
        public float Timestamp => _timestamp;
        public bool IsResolved { get; set; } // Wether the order has been served or not
        public bool IsCancelled { get; set; } // Wether the client has been killed or not
        public Action OnResolve { get => _onResolve; set => _onResolve = value; }
        public Action OnCancel { get => _onCancel; set => _onCancel = value; }

        #endregion

        #region Fields


        private Client _owner;
        private SO_Recipe _recipe;
        private float _timestamp; // Time at which the order was taken (accepted by the player)

        private Action _onResolve;
        private Action _onCancel;

        #endregion

        #region Methods

        public Order(Client owner, SO_Recipe recipe)
        {
            _owner = owner;
            _recipe = recipe;
            _timestamp = Time.time;
        }

        public void Resolve()
        {
            _onResolve?.Invoke();
            IsResolved = true;
            _owner.ResolveOrder();
        }

        public void Cancel()
        {
            _onCancel?.Invoke();
            IsCancelled = true;
        }

        #endregion

    }
}