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
        #endregion

        #region Methods

        private void Start()
        {
            Reset();
            ReferenceManager.Get<ShiftManager>().OnNextShift += Reset;
        }

        private void Reset()
        {
            for (int i = 0; i < _startSpawnIterations; i++)
                TrySpawn();
        }

        public override void Interact(PlayerInteractor interactor)
        {
            base.Interact(interactor);
            //TrySpawn();
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

        public void Spawn()
        {
            foreach (var request in _itemsToSpawn)
            {
                for (int i = 0; i < request.count; i++)
                {
                    //GameObject go = ReferenceManager.Instance.GetManager<ItemManager>().TrySpawn(request.item.transform.gameObject);
                    
                    //Item item = go.GetComponent<Item>();
                    //bool couldBePlaced= false;
                    foreach (var slot in _slotsToSpawn)
                    {
                        if (!slot.IsFree) continue;
                        GameObject go = ReferenceManager.Instance.GetManager<ItemManager>().TrySpawn(request.item.transform.gameObject, slot.transform.position);
                        Item item = go.GetComponent<Item>();
                        
                        item.SetAnimationMode(false);
                        slot.Grab(item);
                        
                        //item.transform.position = slot.transform.position;
                        //couldBePlaced = true;
                        break;
                    }
                   // if (!couldBePlaced) go.transform.position = transform.position;
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