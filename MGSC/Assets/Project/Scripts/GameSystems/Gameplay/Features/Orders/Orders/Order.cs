using UnityEngine;
using Cooking;
using System;
using Clients;
using Orders;

public class Order
{
    #region Properties

    public Client             Owner            => _owner;
    public SO_Recipe          Recipe           => _recipe;
    public EClientType?       Drink            { get; set; }
    public float              Timestamp        => _timestamp;
    public int                ID               => _id;
    public bool               IsResolved       { get; set; } // Wether the order has been served or not
    public bool               IsCancelled      { get; set; } // Wether the client has been killed or not
    public Orders.ServingInfo ServingInfo      { get; set; }
    public Orders.OrderRating OrderRating      { get; set; }
    public float              PatienceDuration { get; set; } = 60f;
    public float              SeatedTimestamp  { get; set; }

    #endregion

    #region Fields


    private Client _owner;
    private SO_Recipe _recipe;
    private float _timestamp; // Time at which the order was taken (accepted by the player)
    private int _id;

    private Action _onResolve;
    private Action _onCancel;

    #endregion

    #region Methods

    public Order() { }

    public Order(Client owner, SO_Recipe recipe, int id)
    {
        _owner     = owner;
        _recipe    = recipe;
        _timestamp = Time.time;
        _id        = id;
    }

    public void SetDrink(EClientType? drink) => Drink = drink;
    
    public void Resolve(Orders.ServingInfo info)
    {
        ServingInfo = info;
        OrderRating = OrderEvaluator.Evaluate(this);
        IsResolved  = true;
        _onResolve?.Invoke();
    }

    public void Cancel()
    {
        _onCancel?.Invoke();
        IsCancelled = true;
    }

    #endregion

}
