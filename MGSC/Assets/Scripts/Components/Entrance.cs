using System.Collections;
using UnityEngine;
using UnityEngine.Events;

    /// <summary>
    /// Entrance of the cafeteria
    /// </summary>
    public class Entrance : MonoBehaviour
    {
        #region Properties
        public UnityEvent OnClientEnter => _onClientEnter;
        #endregion

        #region Fields
        [Space]
        [Header("Entrance events")]
        [Tooltip("Invoked when a client is spawned")]
        [SerializeField] private UnityEvent _onClientEnter;
        
        #endregion

        #region Methods
        

        #endregion
        
    }