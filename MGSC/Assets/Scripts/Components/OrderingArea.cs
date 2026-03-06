using Features;
using System;
using UnityEngine;

public class OrderingArea : MonoBehaviour, IFeatureCollection
{
    #region Properties
    public FeatureCollection Collection { get; set; } = new();

    public ClientSeat ClientSeat { get; set; }
    public OrderingCounter OrderingCounter { get; set; }
    #endregion

    #region Fields

    public Transform SeatTransform;

    #endregion

    private void Awake()
    {
        ClientSeat = new(this, SeatTransform);
        OrderingCounter = new OrderingCounter(this);
        BindEvents();
    }

    private void BindEvents()
    {
        
    }
}