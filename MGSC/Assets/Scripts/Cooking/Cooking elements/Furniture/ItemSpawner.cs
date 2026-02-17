using Controller;
using System.Collections.Generic;
using GameSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// Spawns the specified set of items when interacted with
    /// </summary>
    public class ItemSpawner : Interactable
    {
        #region Properties
        public UnityEvent OnTrySpawn;
        public UnityEvent OnSpawn;
        #endregion

        #region Fields
        [Header("Spawner parameters")]
        [Tooltip("How long the spawner takes to be usable again")]
        [SerializeField] private float _cooldown;
        [Tooltip("What items should be spawned upon interaction")]
        [SerializeField] private List<ItemSpawnRequest> _itemsToSpawn;
        [Tooltip("Slots on which the items will be spawned. If no more slot is available, they will spawn on the spawner's transform. The slots should be ordered")]
        [SerializeField] private List<SlotContainer> _slotsToSpawn;
        [SerializeField] private int _startSpawnIterations = 1;

        private float _timeOfLastSpawn = float.MinValue;

        private ShiftManager _shiftManager;
        private ItemManager _itemManager;
        #endregion

        #region Methods

        private void Start()
        {
            if (!Reference.TryGetManager(out _itemManager))
                Debug.LogWarning("Item manager is null");
            if (Reference.TryGetManager(out _shiftManager))
                _shiftManager.OnNextShift += ResetContent;

            ResetContent();
        }

        private void ResetContent()
        {
            foreach (var slot  in _slotsToSpawn)
            {
                slot.Empty();
            }
            for (int i = 0; i < _startSpawnIterations; i++)
                Spawn(false);
        }

        public override bool RequestInteraction(PlayerInteractor interactor)
        {
            return base.RequestInteraction(interactor);
        }

        public void TrySpawn()
        {
            OnTrySpawn.Invoke();
            if ((Time.time - _timeOfLastSpawn) >= _cooldown)
            {
                Spawn();
                _timeOfLastSpawn = Time.time;
            }
        }

        public void Spawn(bool enableFeedbacks = true)
        {
            foreach (var request in _itemsToSpawn)
            {
                if (request.item == null) continue;
                for (int i = 0; i < request.count; i++)
                {
                    foreach (var slot in _slotsToSpawn)
                    {
                        if (!slot.IsFree) continue;
                        GameObject go = _itemManager.TrySpawn(request.item.transform.gameObject);

                        Item item = go.GetComponent<Item>();
                        item.SetAnimationMode(false);
                        if (!enableFeedbacks)
                            slot.RequestGrab(item);
                        else
                            slot.RequestGrab(item);

                        item.transform.position = slot.transform.position;
                        break;
                    }
                }
            }
            
            OnSpawn.Invoke();
            
        }

        #endregion

        [System.Serializable]
        private struct ItemSpawnRequest
        {
            public Item item;
            public int count;
        }
    }

}