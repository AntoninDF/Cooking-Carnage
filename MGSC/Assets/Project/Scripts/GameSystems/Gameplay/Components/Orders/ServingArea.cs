using Features;
using System;
using System.Runtime.CompilerServices;
using Clients;
using Cooking;
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
            Debug.LogWarning("Please assign a meal slot on the serving area component of " + name);
            return;
        }

        MealSlot.Container.Filter += (obj) =>
        {
            if (obj is not F_Grabbable grab) return false;
            if (!grab.TryGetSibling(out Servable temp, true && ClientSeat.SittingClient != null)) return false;
            if (!Globals.Gameplay.AllowClientServing) return false;
            return true;
        };

        DrinkSlot.Container.Filter += obj =>
                                      {
                                          if (obj is not F_Grabbable grab) return false;
                                          if (grab.Owner is not Drink) return false;
                                          if (!Globals.Gameplay.AllowClientServing) return false;
                                          return true;
                                      };
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

        if (!DrinkExpected)
        {
            var current = MealSlot?.Container?.HeldItem;
            if (current != null && current != _lastHeldItem)
            {
                _lastHeldItem               = current;
                MealSlot.Container.IsLocked = true;
                ClientSeat.OnMealReady?.Invoke();
            }
            if (current == null)
                _lastHeldItem = null;
            return;
        }

        if (MealSlot.Container.IsLocked) return; 

        var meal  = MealSlot?.Container?.HeldItem;
        var drink = DrinkSlot?.Container?.HeldItem;

        if (meal != null && drink != null)
        {
            MealSlot.Container.IsLocked  = true;
            DrinkSlot.Container.IsLocked = true;
            ClientSeat.OnMealReady?.Invoke();
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