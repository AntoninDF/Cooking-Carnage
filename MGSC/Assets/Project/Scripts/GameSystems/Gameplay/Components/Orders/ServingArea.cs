using Features;
using System;
using System.Runtime.CompilerServices;
using Cooking;
using UnityEngine;

public class ServingArea : MonoBehaviour, IFeatureCollection
{
    #region Properties
    
    public FeatureCollection Collection     { get; set; } = new();
    public ClientSeat        ClientSeat     { get; set; }
    
    #endregion

    #region Fields
    
    public SlotContainer MealSlot;
    
    private bool _watching = false;
    private void StartWatchingSlot() => _watching = true;
    private void StopWatchingSlot()
    {
        _lastHeldItem = null; 
        _watching     = false;
    }

    private F_Grabbable _lastHeldItem = null;
    
    #endregion
    
    
    
    private void Start()
    {

        if (MealSlot == null)
        {
            Debug.LogWarning("Please assign a meal slot on the serving area component of " + name);
            return;
        }

        MealSlot.Container.Filter += (obj) =>
        {
            return obj is F_Grabbable grab && grab.TryGetSibling(out Servable temp, true && ClientSeat.SittingClient != null);
        };
    }

    public void Initialize(ClientSeat clientSeat)
    {
        ClientSeat                    =  clientSeat;
        ClientSeat.OnClientRegister   += _ => StartWatchingSlot();
        ClientSeat.OnClientUnregister += _ => StopWatchingSlot();
        ClientSeat.OnClientLeaving    += () => MealSlot.Container.IsLocked = false;

    }

    public void Update()
    {
        if (!_watching) return;
        var current = MealSlot?.Container?.HeldItem;
        if (current != null && current != _lastHeldItem)
        {
            _lastHeldItem               = current;
            MealSlot.Container.IsLocked = true;
            //Debug.Log("[ServingArea] New item detected in slot, invoking OnMealReady");
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