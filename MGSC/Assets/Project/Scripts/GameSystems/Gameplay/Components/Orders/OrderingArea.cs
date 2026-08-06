using Features;
using System;
using System.Collections.Generic;
using Clients;
using Cooking;
using Orders;
using UnityEngine;
using UnityEngine.Events;

public class OrderingArea : MonoBehaviour, IFeatureCollection
{
    #region Properties
    public FeatureCollection Collection { get; set; } = new();
    public ClientSeat ClientSeat { get; set; }
    public OrderingCounter OrderingCounter { get; set; }
    public Order NextOrder { get; set; }

    #endregion

    #region Fields
    
    
    public Transform SeatTransform;
    public Transform SeatParent;
    public ServingArea ServingArea;
    public InteractionButton KillButton;
    
    
    public ClientSeatHeightConfig ServingAreaConfig;
    public UnityEvent OnCustomerKilled;
    
    
    [SerializeField] private OrderDisplayer _OrderDisplayer;
    [SerializeField] private Transform _slotParent;
   

    #endregion

    private void Awake()
    {
        if (ServingArea == null || SeatTransform == null || SeatParent == null)
        {
            Debug.LogWarning($"[OrderingArea] A field is missing on {name}, initialization failed");
        }
        ClientSeat = new(this, SeatTransform, SeatParent, ServingAreaConfig);
        OrderingCounter = new(this);
        ServingArea.Initialize(ClientSeat);
        BindEvents();
        GetDropSlots();
    }

    void Start()
    {
        if (KillButton != null)
            KillButton.OnInteraction += _ => KillClient();
    }

    private void BindEvents()
    {
        ClientSeat.OnClientRegister += RequestOrder;
        ClientSeat.OnClientSitting += AssignOrder;
        ClientSeat.OnClientSitting += (client) =>
        {
            client.BeginOrderTimer();
            client.OnOrderTimeProgress += _OrderDisplayer.UpdateTimeSlider;
            _OrderDisplayer.DisplayOrder(client.CurrentOrder.Recipe);
        };
         
    }

    private void GetDropSlots()
    {
        if (_slotParent == null) return;
        for (int i = 0; i < _slotParent.childCount; i++)
        {
            if (_slotParent.GetChild(i).TryGetComponent(out SlotContainer slotContainer))
            {
               if(slotContainer.gameObject.activeSelf) ClientSeat.dropSlots.Add(slotContainer);
            }
        }
    }

    public void RequestOrder(Client client)
    {
        if (client == null) return;

        NextOrder = OrderingCounter.GetNewOrder(client);
    }
    
    public void AssignOrder(Client client)
    {
        if (client == null) return;
        if (NextOrder == null)
            NextOrder = OrderingCounter.GetNewOrder(client);
        client.TryAcceptOrder(NextOrder);
    }

    public void KillClient()
    {
        if (!Globals.Gameplay.AllowClientKilling)
            return;
        Client client = ClientSeat.RegisteredClient;
        if (client != null && ClientSeat.SittingClient != null)
        {
            client.Cooked.Trigger();
            GameSystem.TimerManager.RemoveTimer("OrderTimer_" + client.UniqueID);
            ClientSeat.OnClientKilled?.Invoke();
            OnCustomerKilled?.Invoke();
        }
        
    }
    
    
    private void OnDrawGizmos()
    {
        float baseHandleHeight = 1f;
        float increment = 0.2f;
        int handleCount = 0;
        float GetY()
        {
            handleCount++;
            return baseHandleHeight + (handleCount - 1) * increment;
        }
        if (!Application.isPlaying) return;
        if (ClientSeat == null) return;
        
        if (ClientSeat.SittingClient != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.1f);
            Gizmos.DrawSphere(ClientSeat.SittingClient.transform.position + Vector3.up * 1f, 0.1f);
        }
        else if (ClientSeat.RegisteredClient != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.1f);
            Gizmos.DrawSphere(ClientSeat.RegisteredClient.transform.position + Vector3.up * 1f, 0.1f);
        }

        // Display current client
        Gizmos.color = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * GetY(), "Client : " + (ClientSeat.RegisteredClient != null ? ClientSeat.RegisteredClient.Type : "null"));

        // Display current meal
        Gizmos.color = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * GetY(), "Current meal : " + (ServingArea.MealSlot.HeldItem != null ? (ServingArea.MealSlot.HeldItem.Owner as Plate).Recipe.Name : "null"));

        // Display seat height
        Gizmos.color = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * GetY(), "Seat height : " + ClientSeat.ClientSeatPosition(ClientSeat.RegisteredClient));
    }
}