using Features;
using System;
using Clients;
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
    
    public ClientSeatHeightConfig ServingAreaConfig;
    
    public UnityEvent OnCustomerKilled;

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
    }

    private void BindEvents()
    {
        ClientSeat.OnClientRegister += RequestOrder;
        ClientSeat.OnClientSitting += AssignOrder;
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
        Client client = ClientSeat.RegisteredClient;
        if (client != null && ClientSeat.SittingClient != null)
        {
            client.Cooked.Trigger();
            OnCustomerKilled?.Invoke();
        }
        
    }
    
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (ClientSeat == null) return;
        if (ClientSeat.SittingClient != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(transform.position, transform.localScale * 1.01f);
            Gizmos.DrawSphere(ClientSeat.SittingClient.transform.position + Vector3.up * 1f, 0.1f);
        }
        else if (ClientSeat.RegisteredClient != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(transform.position, transform.localScale * 1.01f);
            Gizmos.DrawSphere(ClientSeat.RegisteredClient.transform.position + Vector3.up * 1f, 0.1f);
        }
    }
}