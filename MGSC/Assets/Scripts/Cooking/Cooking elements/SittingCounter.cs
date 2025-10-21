using UnityEngine;

namespace Client
{

    public class SittingCounter : MonoBehaviour
    {
        #region Properties
        public float ReachDistance => _reachDistance;
        public Transform SittingSpot => _sittingSpot;
        public bool IsFree => _currentClient == null;

        #endregion

        #region Fields
        [Tooltip("From how far can a client detect this sitting spot and use it. The biggest reach distance between this distance and the client's reach distance will be used.")]
        [SerializeField] private float _reachDistance = 1.0f;
        [Tooltip("Spot where the clients will wait for their order to be taken")]
        [SerializeField] private Transform _sittingSpot;

        private Client _currentClient;
        #endregion

        #region Methods

        public void Register(Client client)
        {
            _currentClient = client;
        }
        #endregion

    }

}