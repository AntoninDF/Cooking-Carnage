using UnityEngine;
using UnityEngine.Events;

namespace Clients
{

    public class Counter : MonoBehaviour
    {
        #region Properties
        public float ReachDistance => _reachDistance;
        public Transform UserSpot => _userSpot;
        public bool IsFree => _currentClient == null;
        public UnityEvent OnUserRegister => _onUserRegister;
        public UnityEvent OnUserUnregister => _onUserUnregister;

        #endregion

        #region Fields
        [Header("Counter references")]
        [Tooltip("Spot where the user/client will got when using this counter")]
        [SerializeField] protected Transform _userSpot;
        [Space]
        [Header("Counter parameters")]
        [Tooltip("From how far can a client detect this sitting spot and use it. The biggest reach distance between this distance and the client's reach distance will be used.")]
        [SerializeField] protected float _reachDistance = 1.0f;
        [Space]
        [Header("Counter events")]
        [Tooltip("Invoked when a user/client reserves this counter")]
        [SerializeField] private UnityEvent _onUserRegister;
        [Tooltip("Invoked when a user/client uses this counter")]
        [SerializeField] private UnityEvent _onUserInteraction;
        [Tooltip("Invoked when a user/client stops using this counter")]
        [SerializeField] private UnityEvent _onUserUnregister;

        protected Client _currentClient;
        #endregion

        #region Methods

        private void Start()
        {
            if (_userSpot == null)
                Debug.LogError("Please reference a user spot for this counter '" + name + "'");
        }
        public void Register(Client client)
        {
            _currentClient = client;
            _onUserRegister?.Invoke();
        }
        
        public void Interaction()
        {
            _onUserInteraction?.Invoke();
        }

        public void Free()
        {
            _currentClient = null;
            _onUserUnregister?.Invoke();
        }
        #endregion

    }

}