using Cooking;
using GameSystem;
using NUnit.Framework;
using Orders;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Clients
{

    public class SittingCounter : Counter
    {
        #region Properties
        public UnityEvent OnKill => _onKill;
        public UnityEvent OnServe => _onServe;
        public Meal ServedMeal => _mealSlot?.HeldItem as Meal;

        #endregion

        #region Fields
        [Header("References")]
        [Tooltip("Slots on which the items will be spawned. If no more slot is available, they will spawn on the spawner's transform. The slots should be ordered")]
        [SerializeField] private List<SlotContainer> _slotsToSpawn;
        [Tooltip("Slot on which the meal will be served.")]
        [SerializeField] private SlotContainer _mealSlot;
        [Space]
        [Header("Specific events")]
        [Tooltip("This event will be invoked when a client is successfully killed")]
        [SerializeField] private UnityEvent _onKill;
        [Tooltip("This event will be invoked when a client is served")]
        [SerializeField] private UnityEvent _onServe;

        private OrderManager _orderManager;
        #endregion

        #region Methods

        private void Start()
        {
            EnforceSlots();
            _mealSlot.OnGrab.AddListener(Serve);
            _orderManager = ReferenceManager.Instance.GetManager<OrderManager>();
        }

        private void EnforceSlots()
        {
            if (_slotsToSpawn == null || _slotsToSpawn.Count == 0)
                Debug.LogError("Please reference several slots on the sitting counter '" + name + "' for ingredients to be put on");
            if (_mealSlot == null)
                Debug.LogError("Please reference a slot on the sitting counter '" + name + "' for the meal to be served");


            ContainerFilter filter;
            foreach (var slot in _slotsToSpawn)
            {
                if (slot.Filter == null)
                    filter = slot.AddComponent<ContainerFilter>();
                else
                    filter = slot.GetComponent<ContainerFilter>();
                filter.SetAll(false);
                filter.item = true;
                filter.ingredient = true;
                filter.filterOperator = ContainerFilter.ELogicOperator.And;
            }
            if (_mealSlot.Filter == null)
                filter = _mealSlot.AddComponent<ContainerFilter>();
            else
                filter = _mealSlot.GetComponent<ContainerFilter>();
            filter.SetAll(false);
            filter.item = true;
            filter.meal = true;
            filter.filterOperator = ContainerFilter.ELogicOperator.And;
        }

        private void Serve(IGrabbable item)
        {
            Meal meal = item as Meal;
            _onServe?.Invoke();
            ServingInfo info = new ServingInfo(meal);
            _orderManager.ResolveOrder(_currentClient.Order, info);
        }

        public void TryKill()
        {
            if (IsFree) return;
            Kill();
        }

        private void Kill()
        {
            AddLoot(_currentClient.IngredientsToDrop);
            Debug.Log("Number of ingredients to drop : " + _currentClient.IngredientsToDrop.Count);
            _currentClient.Kill();
            _currentClient = null;
            _onKill?.Invoke();

        }

        private void AddLoot(List<SO_Ingredient> loot)
        {
            foreach (var item in loot)
            {
                Spawn(item);
            }
        }
        public void Spawn(SO_Ingredient ingredient)
        {
            Debug.Log("Spawning item...");
            if (_slotsToSpawn == null || _slotsToSpawn.Count == 0)
            {
                Debug.LogError("[SittingCounter] Spawn : Couldn't find any slot to put the ingredients. Please reference them in the inspector");
            }

            GameObject go = GameObject.Instantiate(ingredient.Prefab);
            Ingredient item = go.GetComponent<Ingredient>();
            bool couldBePlaced = false;
            foreach (var slot in _slotsToSpawn)
            {
                if (!slot.IsEmpty) continue;
                slot.Grab(item);
                item.transform.position = slot.transform.position;
                couldBePlaced = true;
                Debug.Log("Item placed on slot");
                break;
            }
            if (!couldBePlaced) go.transform.position = transform.position;
        }
        #endregion

    }

}