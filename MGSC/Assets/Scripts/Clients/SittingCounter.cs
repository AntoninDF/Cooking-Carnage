using Cooking;
using GameSystem;
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
                {
                    filter = slot.AddComponent<ContainerFilter>();
                }
                else
                {
                    filter = slot.GetComponent<ContainerFilter>();
                }
                filter.Clear();
                filter.AddFilter(ContainerFilter.EFilterCondition.MustBe, ContainerFilter.EItemType.Ingredient);
            }
            if (_mealSlot.Filter == null)
                filter = _mealSlot.AddComponent<ContainerFilter>();
            else
                filter = _mealSlot.GetComponent<ContainerFilter>();
            filter.Clear();
            filter.AddFilter(ContainerFilter.EFilterCondition.MustBe, ContainerFilter.EItemType.Meal);
        }

        private void Serve(IGrabbable item)
        {
            if (_currentClient == null) return;
            Meal meal = item as Meal;
            //Debug.Log("Meal is " + (meal==null?"null":meal));
            _onServe?.Invoke();
            ServingInfo info = new ServingInfo(meal);
            _orderManager.ResolveOrder(_currentClient.Order, info);
            _mealSlot.Drop(item);
            Destroy(meal.gameObject);
            Free();
        }

        public void TryKill()
        {
            if (IsFree) return;

            if (_currentClient.CheckClientCounterDistance(this))
            {
                Kill();
            }
        }

        private void Kill()
        {
            AddLoot(_currentClient.IngredientToDrop, _currentClient.Model.dropRate);
            _orderManager.CancelOrder(_currentClient.Order);
            _currentClient.Kill();
            Free();
            _onKill?.Invoke();

        }

        private void AddLoot(SO_Ingredient loot, Vector2 dropRate)
        {
            int count = Mathf.RoundToInt(Random.Range(dropRate.x, dropRate.y));
            for (int i = 0; i < count; i++)
            {
                Spawn(loot);
            }
        }
        public void Spawn(SO_Ingredient ingredient)
        {
            if (_slotsToSpawn == null || _slotsToSpawn.Count == 0)
            {
                Debug.LogError("[SittingCounter] Spawn : Couldn't find any slot to put the ingredients. Please reference them in the inspector");
            }

            GameObject go = GameObject.Instantiate(ingredient.Prefab);
            Ingredient item = go.GetComponent<Ingredient>();
            item.SetAnimationMode(false);
            bool couldBePlaced = false;
            foreach (var slot in _slotsToSpawn)
            {
                if (!slot.IsFree) continue;
                slot.Grab(item);
                item.transform.position = slot.transform.position;
                couldBePlaced = true;
                break;
            }
            if (!couldBePlaced) go.transform.position = transform.position;
        }
        #endregion

    }

}