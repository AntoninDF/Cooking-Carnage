using System;
using UnityEngine;

namespace Client
{

    public class OrderingCounter : MonoBehaviour
    {
        #region Properties
        public float ReachDistance => _reachDistance;
        public Transform OrderingSpot => _orderingSpot;

        #endregion

        #region Fields
        [Tooltip("From how far can a client detect this ordering counter and use it. The biggest reach distance between this distance and the client's reach distance will be used.")]
        [SerializeField] private float _reachDistance = 1.0f;
        [Tooltip("Spot where the clients will wait for their order to be taken")]
        [SerializeField] private Transform _orderingSpot;

        private Client _currentClient;

        #endregion

        #region Methods

        private void Start()
        {
            if (_orderingSpot == null)
                Debug.LogError("Please reference an ordering spot for the ordering counter '" + name + "'");
        }

        public void Register(Client client)
        {
            _currentClient = client;
        }

        public void TryAcceptOrder()
        {
            if (_currentClient == null) return;
            AcceptOrder();
            return;
        }

        private void AcceptOrder()
        {
            _currentClient.AcceptOrder();
            _currentClient = null;
        }

        #endregion

    }

}