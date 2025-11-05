using Controller;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// Spawns the specified set of items when interacted with
    /// </summary>
    public class ItemSpawner : Interactable
    {
        #region Properties

        #endregion

        #region Fields
        [Header("Spawner parameters")]
        [Tooltip("How long the spawner takes to be usable again")]
        [SerializeField] private float _cooldown;
        [Tooltip("What items should be spawned upon interaction")]
        [SerializeField] private List<ItemSpawnRequest> _itemsToSpawn;
        [Tooltip("Slots on which the items will be spawned. If no more slot is available, they will spawn on the spawner's transform. The slots should be ordered")]
        [SerializeField] private List<SlotContainer> _slotsToSpawn;

        private float _timeOfLastSpawn = float.MinValue;
        #endregion

        #region Methods

        private void Start()
        {
            TrySpawn();
            foreach (var slot in _slotsToSpawn)
            {
                slot.OnDrop.AddListener((item) => TrySpawn());
            }
        }

        public override void Interact(PlayerInteractor interactor)
        {
            base.Interact(interactor);
            //TrySpawn();
        }

        public void TrySpawn()
        {
            if ((Time.time - _timeOfLastSpawn) >= _cooldown)
            {
                Spawn();
                _timeOfLastSpawn = Time.time;
            }
        }

        public void Spawn()
        {
            foreach (var request in _itemsToSpawn)
            {
                for (int i = 0; i < request.count; i++)
                {
                    GameObject go = GameObject.Instantiate(request.item.transform.gameObject);
                    Item item = go.GetComponent<Item>();
                    bool couldBePlaced= false;
                    foreach (var slot in _slotsToSpawn)
                    {
                        if (!slot.IsFree) continue;
                        item.SetAnimationMode(false);
                        slot.Grab(item);
                        item.transform.position = slot.transform.position;
                        couldBePlaced = true;
                        break;
                    }
                    if (!couldBePlaced) go.transform.position = transform.position;
                }
            }
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