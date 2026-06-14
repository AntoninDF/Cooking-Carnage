using Features;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This part of the client class handles the client's behaviour.
/// Movement is handled externally : See ClientMovement.cs
/// The client generation is defined in ClientGenerator.cs. It is joined to the Client through partial class
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public partial class Client : MonoBehaviour, IFeatureCollection
{
    #region Properties
    public FeatureCollection Collection { get; set; } = new();
    public Action<Order> OnNewOrder {  get; set; }
    public Action<ClientSeat> OnSeatRegister { get; set; }
    public Action<ClientSeat> OnSeatUnregister { get; set; }
    public ClientBehaviour Behaviour { get; set; }
    public Order CurrentOrder
    {
        get => _currentOrder;
        set
        {
            Debug.Log("Current order is accessed");
            if (_currentOrder == value) return;
            _currentOrder = value;
            if (value != null)
            {
                Debug.Log("Current order is set");
                OnNewOrder?.Invoke(value);
            }
        }
    }
    public ClientSeat CurrentSeat
    {
        get => _currentSeat;
        set
        {
            if (_currentSeat == value) return;
            OnSeatUnregister?.Invoke(_currentSeat);
            if (value != null)
            {
                OnSeatRegister?.Invoke(value);
            }

            _currentSeat = value;
        }
    }

    #endregion

    #region Fields
    Order _currentOrder;
    ClientSeat _currentSeat;


    #endregion

    private void Awake()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        Behaviour = new(this, agent);
        // OnNewOrder += TryAcceptOrder;
        OnSeatRegister += OnNewSeatRegister;
    }

    private void Start()
    {
        // When a client appears, he is hungry by nature.
        if (CurrentOrder == null)
        {
            Behaviour.GoToOrderingCounter(Sit);
        }
    }

    void Update()
    {
        Collection.Update();
    }

    public void Sit(ClientSeat seat)
    {
        if (seat == null) return;

        seat.TrySit(this);
    }

    private void OnNewSeatRegister(ClientSeat seat)
    {
        if (seat.TryGetSibling(out OrderingCounter orderingCounter))
        {
            if (CurrentOrder == null)
            {
                //orderingCounter.OnNewOrder += TryAcceptOrder;
                return;
            }
        }
        if (seat.TryGetSibling(out ServingCounter servingCounter))
        {
            servingCounter.OnServe += TryAcceptServing;
        }
    }

    public void TryAcceptOrder(Order order)
    {
        if (order == null) return;
        if (CurrentOrder != null) return;
        Debug.Log("Order accepted");
        CurrentOrder = order;
        CurrentSeat.TryUnregister(this);
        Behaviour.GoToServingCounter(Sit);
    }
    private void TryAcceptServing(Servable serving)
    {
        Debug.Log("TryAcceptServing");
        if (serving == null || !serving.Enabled) return;
        Debug.Log("Something was served");
        CurrentOrder.Resolve(serving);
        Behaviour.GoToCounter<Exit>();
    }
    private void OnOrderResolution()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // Draw order
        if (CurrentOrder != null)
        {
            Gizmos.color = Color.white;
            Handles.Label(transform.position + Vector3.up * 1f, "HAS ORDER");
        }
        else
        {
            Gizmos.color = Color.Lerp(Color.white, Color.red, 0.3f);
            Handles.Label(transform.position + Vector3.up * 1f, "NO ORDER");
        }

        if (CurrentSeat != null)
        {
            Gizmos.color = Color.Lerp(Color.white, Color.blue, 0.3f);
            Handles.Label(transform.position + Vector3.up * 1.2f, "CurrentSeat is " + (CurrentSeat.Owner as UnityEngine.Object).name);
        }
    }
    // Find ClientTargets



    /*

    The client looks for seats.
    If none are found, they stay in place.
    If there are seats available, the client asks themselves : Am I hungry ?
    If so, they look for a seat associated to a OrderingCounter
    Otherwise they select another seat.


    */
}