using UnityEngine;
using Cooking;
using System;
using Orders;

public class Order
{
    #region Properties

    public Client Owner => _owner;
    public SO_Recipe Recipe => _recipe;
    public float Timestamp => _timestamp;
    public int ID => _id;
    public bool IsResolved { get; set; } // Wether the order has been served or not
    public bool IsCancelled { get; set; } // Wether the client has been killed or not
    public Action OnResolve { get => _onResolve; set => _onResolve = value; }
    public Action OnCancel { get => _onCancel; set => _onCancel = value; }
    public Orders.ServingInfo ServingInfo { get; set; }
    public Orders.OrderRating OrderRating { get; set; }

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

    public Order()
    {
    }
    public Order(Client owner, SO_Recipe recipe, int id)
    {
        _owner = owner;
        _recipe = recipe;
        _timestamp = Time.time;
        _id = id;
    }

    public void Resolve(Orders.ServingInfo info, Orders.OrderRating rating)
    {
        ServingInfo = info;
        OrderRating = rating;
        _onResolve?.Invoke();
        IsResolved = true;
        //_owner.ResolveOrder(info, rating);
    }
    public void Resolve(Servable serving)
    {
        OrderRating = OrderEvaluator.Evaluate(this);
        _onResolve?.Invoke();
        IsResolved = true;
        //_owner.ResolveOrder(info, rating);
    }

    public void Cancel()
    {
        _onCancel?.Invoke();
        IsCancelled = true;
    }

    #endregion

}
