using Features;
using System;
using Clients;
using UnityEngine;

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

    #endregion

    private void Awake()
    {
        ClientSeat = new(this, SeatTransform);
        OrderingCounter = new(this);
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

        NextOrder = OrderingCounter.GetNewOrder();
    }
    public void AssignOrder(Client client)
    {
        if (client == null) return;
        if (NextOrder == null)
            NextOrder = OrderingCounter.GetNewOrder();
        client.TryAcceptOrder(NextOrder);
    }


    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
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