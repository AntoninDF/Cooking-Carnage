using Features;
using System;
using Clients;
using Project.Scripts.GameSystems.Gameplay.Components.Drinks;
using UnityEngine;

public class ServingArea : MonoBehaviour, IFeatureCollection
{
    #region Properties
    
    public FeatureCollection Collection     { get; set; } = new();
    public ClientSeat        ClientSeat     { get; set; }
    
    private bool DrinkExpected => ClientSeat?.SittingClient?.CurrentOrder?.Drink != null;
    
    #endregion

    #region Fields
    
    public SlotContainer MealSlot;
    public SlotContainer DrinkSlot;
    
    private bool _watching = false;
    private void StartWatchingSlot()
    {
        //Debug.LogWarning("[ServingArea] StartWatchingSlot() appelé — watching = true");
        _watching = true;
    }

    private void StopWatchingSlot()
    {
        _lastHeldItem = null; 
        _watching     = false;
    }

    private F_Grabbable _lastHeldItem = null;
    
    #endregion
    
    
    
    private void Start()
    {
        MealSlot.Container.IsLocked  = true;
        DrinkSlot.Container.IsLocked = true;

        ClientSeat.OnClientSitting += OnClientSitting; 
        ClientSeat.OnClientLeaving += ()=>
                                      {
                                          MealSlot.Container.IsLocked  = true;
                                          DrinkSlot.Container.IsLocked = true;
                                      };
        

        if (MealSlot == null)
        {
            //Debug.LogWarning("Please assign a meal slot on the serving area component of " + name);
            return;
        }

        MealSlot.Container.Filter  += BuildSlotFilter(MealSlot);
        DrinkSlot.Container.Filter += BuildSlotFilter(DrinkSlot);
        
        
        if (MealSlot.Container == DrinkSlot.Container)
            Debug.LogError($"[ServingArea] MealSlot et DrinkSlot référencent le MÊME container sur {name} — vérifier l'Inspector !");
    }

    private void OnClientSitting(Client obj)
    {
        MealSlot.Container.IsLocked  = false;
        DrinkSlot.Container.IsLocked = !DrinkExpected;
    }
   

    public void Initialize(ClientSeat clientSeat)
    {
        ClientSeat                    =  clientSeat;
        ClientSeat.OnClientRegister   += _ => StartWatchingSlot();
        ClientSeat.OnClientUnregister += _ => StopWatchingSlot();
        ClientSeat.OnClientLeaving    += () =>
                                         {
                                             MealSlot.Container.IsLocked  = false;
                                             DrinkSlot.Container.IsLocked = !DrinkExpected;
                                         };
        ClientSeat.OnClientKilled += () =>
                                     {
                                         MealSlot.Container.IsLocked  = true;
                                         DrinkSlot.Container.IsLocked = true;
                                     };

    }


    public void Update()
    {
        if (!_watching) return;

        var meal  = MealSlot?.Container?.HeldItem;
        var drink = DrinkSlot?.Container?.HeldItem;

        if (!DrinkExpected)
        {
            if (meal != null && meal != _lastHeldItem)
            {
                _lastHeldItem               = meal;
                MealSlot.Container.IsLocked = true;
                ClientSeat.OnMealReady?.Invoke();
            }
            else if (meal == null)
            {
                _lastHeldItem = null;
            }
            return;
        }

        if (MealSlot.Container.IsLocked) return;

        if (meal != null && drink != null)
        {
            MealSlot.Container.IsLocked  = true;
            DrinkSlot.Container.IsLocked = true;
            ClientSeat.OnMealReady?.Invoke();
        }
    }
    
    private bool ItemTypeMatchesSlot(SlotContainer slot, F_Grabbable grab)
    {
        if (slot == MealSlot)  return grab.Owner is Plate;
        if (slot == DrinkSlot) return grab.Owner is Drink;
        return false;
    }

    private Func<object, bool> BuildSlotFilter(SlotContainer slot)
    {
        return obj =>
               {
                   if (obj is not F_Grabbable grab) return false;
                   if (!Globals.Authorizations.AllowClientServing) return false;
                   return ItemTypeMatchesSlot(slot, grab);
               };
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