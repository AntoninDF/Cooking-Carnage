using System;
using Features;
using UnityEngine;
using UnityEngine.AI;

namespace Clients
{
    /// <summary>
    /// The Client is only supposed to Create his behavior with the good data
    /// this one is an updated version
    /// </summary>
    // [RequireComponent(typeof(NavMeshAgent))]
    // public partial class Client : MonoBehaviour, IFeatureCollection
    // {
    //     #region Properties
    //     public FeatureCollection Collection { get; set; } = new();
    //     public Action<Order> OnNewOrder {  get; set; }
    //     public Action<ClientSeat> OnSeatRegister { get; set; }
    //     public Action<ClientSeat> OnSeatUnregister { get; set; }
    //     public ClientBehaviour Behaviour { get; set; }
    //     public Order CurrentOrder
    //     {
    //         get => _currentOrder;
    //         set
    //         {
    //             //Debug.Log("Current order is accessed");
    //             if (_currentOrder == value) return;
    //             _currentOrder = value;
    //             if (value != null)
    //             {
    //                 //Debug.Log("Current order is set");
    //                 OnNewOrder?.Invoke(value);
    //             }
    //         }
    //     }
    //     public ClientSeat CurrentSeat
    //     {
    //         get => _currentSeat;
    //         set
    //         {
    //             if (_currentSeat == value) return;
    //             OnSeatUnregister?.Invoke(_currentSeat);
    //             if (value != null)
    //             {
    //                 OnSeatRegister?.Invoke(value);
    //             }
    //
    //             _currentSeat = value;
    //         }
    //     }
    //
    //     #endregion
    //
    //     #region Fields
    //
    //     private Order _currentOrder;
    //     private ClientBehaviour _clientBehaviour;
    //     private NavMeshAgent _navMeshAgent;
    //     private ClientSeat _currentSeat;
    //     private ClientLook _clientLook;
    //
    //     #endregion
    //
    //     private void Awake()
    //     {
    //         Initialize();
    //         
    //         OnSeatRegister += OnNewSeatRegister;
    //     }
    //
    //     void Initialize()
    //     {
    //         _navMeshAgent = GetComponent<NavMeshAgent>();
    //         Behaviour = new(this, transform, _navMeshAgent, _splineComp, _speed);
    //     }
    //
    //    
    //
    //     void Update()
    //     {
    //         Collection.Update();
    //     }
    // }
}
