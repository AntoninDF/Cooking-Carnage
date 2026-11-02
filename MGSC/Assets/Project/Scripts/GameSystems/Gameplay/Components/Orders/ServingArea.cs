using Features;
using System;
using System.Runtime.CompilerServices;
using Cooking;
using UnityEngine;
using UnityEngine.Serialization;

public class ServingArea : MonoBehaviour, IFeatureCollection
{
    #region Properties

    public FeatureCollection Collection { get; set; } = new();
    public ClientSeat ClientSeat { get; set; }
    //public SlotContainer ServingSlot => MealSlot;
    public SlotContainer SecondarySlot => DrinkSlot;
    
    #endregion

    #region Fields
    public SlotContainer ServingSlot;
    public SlotContainer MealSlot;
    public SlotContainer DrinkSlot;

    private bool _watching = false;
    private void StartWatchingSlot() => _watching = true;
    private void StopWatchingSlot()
    {
        _lastHeldItem = null;
        _watching = false;
    }

    private F_Grabbable _lastHeldItem = null;

    #endregion



    private void Start()
    {
        ServingSlot.Container.IsLocked = true;

        ClientSeat.OnClientSitting += (_) => ServingSlot.Container.IsLocked = false;
        ClientSeat.OnClientLeaving += () => ServingSlot.Container.IsLocked = true;


        if (ServingSlot == null)
        {
            Debug.LogWarning("Please assign a meal slot on the serving area component of " + name);
            return;
        }

        ServingSlot.Container.Filter += (obj) =>
        {
            if (obj is not F_Grabbable grab) return false;
            if (!grab.TryGetSibling(out Servable temp, true && ClientSeat.SittingClient != null)) return false;
            if (!Globals.Authorizations.AllowClientServing) return false;
            if (temp.Owner is Drink && ClientSeat.SittingClient?.CurrentOrder.Drink == Clients.EClientType.None) return false; // Make sure drinks are served only if drinks are asked
            if (temp.Owner is Drink && DrinkSlot.HeldItem != null) return false;
            if (temp.Owner is not Drink && MealSlot.HeldItem != null) return false;
            return true;
        };
    }

    public void Initialize(ClientSeat clientSeat)
    {
        ClientSeat = clientSeat;
        ClientSeat.OnClientRegister += _ => StartWatchingSlot();
        ClientSeat.OnClientUnregister += _ => StopWatchingSlot();
        ClientSeat.OnClientLeaving += () => ServingSlot.Container.IsLocked = false;
        ClientSeat.OnClientKilled += () => ServingSlot.Container.IsLocked = true;

    }

    public void Update()
    {
        if (!_watching) return;
        var current = ServingSlot?.Container?.HeldItem;
        if (current != null && current != _lastHeldItem)
        {
            _lastHeldItem = current;
            ServingSlot.Container.IsLocked = true;
            ClientSeat.OnMealReady?.Invoke();
        }
        if (current == null)
            _lastHeldItem = null;
    }
    


    private void OnDrawGizmos()
    {
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
    }
}