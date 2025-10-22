using Cooking;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Client
{

    public class SittingCounter : Counter
    {
        #region Properties
        public UnityEvent OnKill => _onKill;

        #endregion

        #region Fields
        [Header("References")]
        [Tooltip("Slots on which the items will be spawned. If no more slot is available, they will spawn on the spawner's transform. The slots should be ordered")]
        [SerializeField] private List<SlotContainer> _slotsToSpawn;
        [Space]
        [Header("Specific events")]
        [Tooltip("This event will be invoked when a client is successfully killed")]
        [SerializeField] private UnityEvent _onKill;

        #endregion

        #region Methods

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