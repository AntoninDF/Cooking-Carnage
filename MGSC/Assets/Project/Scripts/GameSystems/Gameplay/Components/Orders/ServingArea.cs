using Features;
using System;
using Clients;
using UnityEngine;
using UnityEngine.Serialization;

public class ServingArea : MonoBehaviour, IFeatureCollection
{
    #region Properties
    
    public FeatureCollection Collection     { get; set; } = new();
    public ClientSeat        ClientSeat     { get; set; }
    
    private bool DrinkExpected => ClientSeat?.SittingClient?.CurrentOrder?.Drink != null && ClientSeat.SittingClient.CurrentOrder.Drink != EClientType.None;

    #endregion

    #region Fields

    [FormerlySerializedAs("MealSlot")]
    public MultiSlotContainer ServingSlot;
    
    private bool _watching = false;
    private void StartWatchingSlot() =>  _watching = true;
    private void StopWatchingSlot()
    {
        _lastHeldItem = null; 
        _watching     = false;
    }

    private F_Grabbable _lastHeldItem = null;
    #endregion
    
    private void Start()
    {
        ServingSlot.Container.IsLocked  = true;
        //SecondarySlot.Container.IsLocked = true;

        ClientSeat.OnClientSitting += OnClientSitting; 
        ClientSeat.OnClientLeaving += ()=>
                                      {
                                          ServingSlot.Container.IsLocked  = true;
                                          //SecondarySlot.Container.IsLocked = true;
                                      };

        ServingSlot.Container.Filter  += BuildSlotFilter(ServingSlot);
        //SecondarySlot.Container.Filter += BuildSlotFilter(SecondarySlot);
        
        
        //if (ServingSlot.Container == SecondarySlot.Container)
            //Debug.LogError($"[ServingArea] MealSlot et DrinkSlot référencent le MÊME container sur {name} — vérifier l'Inspector !");
    }

    private void OnClientSitting(Client obj)
    {
        ServingSlot.Container.IsLocked  = false;
        //SecondarySlot.Container.IsLocked = !DrinkExpected;
    }
   

    public void Initialize(ClientSeat clientSeat)
    {
        ClientSeat                    =  clientSeat;
        ClientSeat.OnClientRegister   += _ => StartWatchingSlot();
        ClientSeat.OnClientUnregister += _ => StopWatchingSlot();

        Action lockAll = () => {
            ServingSlot.Container.IsLocked = true;
            //SecondarySlot.Container.IsLocked = true;
        };
        ClientSeat.OnClientLeaving += lockAll;
        ClientSeat.OnClientKilled += lockAll;

    }


    public void Update()
    {
        if (!_watching) return;

        var meal  = ServingSlot?.Container?.HeldItem;
        //var drink = SecondarySlot?.Container?.HeldItem;

        if (!DrinkExpected)
        {
            if (meal != null && meal != _lastHeldItem)
            {
                _lastHeldItem               = meal;
                ServingSlot.Container.IsLocked = true;
                ClientSeat.OnMealReady?.Invoke();
            }
            else if (meal == null)
                _lastHeldItem = null;

            return;
        }

        if (ServingSlot.Container.IsLocked) return;

        if (meal != null)// && drink != null)
        {
            ServingSlot.Container.IsLocked  = true;
            //SecondarySlot.Container.IsLocked = true;
            ClientSeat.OnMealReady?.Invoke();
        }
    }
    
    private bool ItemTypeIsServable(F_Grabbable grab) 
        => (grab.Owner is Plate plate && plate.Cookable.Transformed) || grab.Owner is Drink;

    private Func<object, bool> BuildSlotFilter(MultiSlotContainer slot) { 
    return obj => { if (obj is not F_Grabbable grab) return false;
                if (!Globals.Authorizations.AllowClientServing) return false;
                return ItemTypeIsServable(grab); }; }

#if UNITY_EDITOR
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
#endif
}