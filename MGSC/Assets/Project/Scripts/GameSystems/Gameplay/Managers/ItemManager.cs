using System.Collections.Generic;
using Cooking;
using GameSystem;
using UnityEngine;

public class ItemManager : IManager
{
    #region Properties
    
    #endregion

    #region Fields
    
    List<Item> spawnItems = new List<Item>();
    
    List<Vector3> spawnPositions = new List<Vector3>(); //REFACTO
    private Transform spawnParent; // REFACTO
    
    List<Item> activeItems = new List<Item>();

    // Specifies, for shift X, the maximum amount of items Y that can be spawned. If Y is superior to the amount of items available in the scene, no additional items are spawned
    //                                                                           X   Y
    Dictionary<int, int> _maxSpawnItemsPerShift = new Dictionary<int, int>() { { 0 , 1 } };
    
    private Transform _itemsPooled;
    
    
    
    
    #endregion
    
    public ItemManager()
    {
        Register();
    }
    
    public void Register()
    {
        Reference.SetManager(this);
    }

    public void Unregister()
    {
        Reference.RemoveManager(this);
    }

    public void Init()
    {
        GameObject newRoot = new GameObject("Items Pool");
        _itemsPooled = newRoot.transform;

        //DetectSpawnItems(); // Refacto : The item manager shouldn't handle the first item spawns of a shift. This should be a dedicated script
    }

    public void PostInit()
    {
    }
    
    private void DetectSpawnItems()
    {
        foreach (var item in Reference.GetAll<Item>())
        {
            spawnItems.Add(item);
            spawnPositions.Add(item.transform.position);
        }
        spawnParent = spawnItems.Count > 0 ? spawnItems[0].transform.parent : null;

        /*
        for (var index = spawnItems.Count - 1; index >= 0; index--)
        {
            var initialItem = spawnItems[index];

            UpdateActiveList(initialItem);
        }
        */
    }


    private void UpdateActiveList(Item item)
    {
        if (item.gameObject != null)
        {
            if (item.gameObject.activeSelf)
            {
                if (!activeItems.Contains(item))
                {
                    activeItems.Add(item);
                    this.Log(LogType.Log, item.name + " has been registered as active item");
                }
            }
            else
            {
                if (activeItems.Contains(item))
                {
                    activeItems.Remove(item);
                }
            }
        }
        
    }
    

    private List<Item> correspondingItems;
    
    public GameObject TrySpawn(GameObject item ,Vector3 position = default, Quaternion rotation = default, Transform parent = null)
    {
        if (!item.TryGetComponent<Item>(out Item prefabItem))
        {
            this.Log(LogType.Error, item.name + " cannot be spawned because it isn't an item");
            
            return null;
        }
        
        GameObject spawned = GameObject.Instantiate(item, position, rotation, parent);
        
        UpdateActiveList(item.GetComponent<Item>());
        
        this.Log(LogType.Log, "Spawned " + item.name);
        
        return spawned;
    }
    
    
    public void Despawn(Item itemToDespawn, bool reset = false)
    {
        if (itemToDespawn.gameObject == null)
        {
            this.Log(LogType.Error, itemToDespawn.name + " gameobject not found, probably destroyed ?");
            return;
        }
        if (!itemToDespawn.gameObject.activeSelf)
        {
            this.Log(LogType.Warning, itemToDespawn.name + " is not active and probably shouldn't be despawned  "+itemToDespawn.GetEntityId().ToString());
        }
        itemToDespawn.gameObject.SetActive(false);

        UpdateActiveList(itemToDespawn);
        
        this.Log(LogType.Log, "Despawned " + itemToDespawn.name );
        
        GameObject.Destroy(itemToDespawn.gameObject);
        
    }
    

    public void DestroyAllActive()
    {
        //Debug.Log("Active items : " + activeItems.Count + " (being stored)");
        for (var index = activeItems.Count-1; index >= 0; index--)
        {
            var item = activeItems[index];
            Despawn(item);
        }
        activeItems.Clear();
    }

    public void SpawnInitialItems(int shiftCount)
    {
        this.Log("Spawning initial items");
        int i = 0;
        foreach (var item in spawnItems)
        {
            if (_maxSpawnItemsPerShift.ContainsKey(shiftCount))
                if (_maxSpawnItemsPerShift[shiftCount] <= i)
                    break;
            this.Log(item.transform.position);
            int index = spawnItems.IndexOf(item);
            TrySpawn(item.gameObject, spawnPositions[index], item.transform.rotation, item.transform.parent);
            i++;
        }
    }
    
    #region Item Pooling Archive 
    /*
     
     Dictionary<string, List<Item>> _itemsByID = new Dictionary<string, List<Item>>();
     
     private void TryRemove(Item item, string id=null)
    {
        if (item == null)
        {
            this.Log(LogType.Error, item.name + " can't be removed because it is null");
            return;
        }

        if (id == null)
        {
            id = item.GetEntityId().ToString();
        }

        if (_itemsByID.ContainsKey(id))
        {
            if (_itemsByID[id].Contains(item))
            {
                _itemsByID[id].Remove(item);
                this.Log(LogType.Log, item.name +" has been removed, there is "+ _itemsByID[item.GetEntityId().ToString()].Count+" left");
            }
            else
            {
                this.Log(LogType.Warning, item.name + " can't be removed because it wasn't stored at ID "+item.GetEntityId().ToString());
            }
        }
    }
    List<Item> GetItemsByID(string id)
    {
        if (_itemsByID.ContainsKey(id))
        {
            return _itemsByID[id];
        }
        
        this.Log(LogType.Warning, "No item of ID "+id+ " has ever been stored");
        
        return null;
    }
     
    private void TryStore(Item item, string id =null)
    {
        if (item == null )
        {
            this.Log(LogType.Error, item.name + " can't be stored because it is null");
            return;
        }

        if (id == null)
        {
            id = item.GetEntityId().ToString();
        }
        
        if (!_itemsByID.ContainsKey(id))
        {
            _itemsByID.Add(id, new List<Item>());
            this.Log(LogType.Log, id + " has been added");
        }
        
        if (!_itemsByID[id].Contains(item))
        {
            _itemsByID[id].Add(item);
            this.Log(LogType.Log, "There is now "+ _itemsByID[id].Count + " item of ID "+ id);
        }
        else
        {
            this.Log(LogType.Log, item.name+ " has already been added at ID "+id);
        }
        
    }
    
    public void Despawn(Item itemToStore, bool reset = false)
    {
        if (itemToStore.gameObject == null)
        {
            this.Log(LogType.Error, itemToStore.name + " gameobject not found, probably destroyed ?");
            return;
        }
        if (!itemToStore.gameObject.activeSelf)
        {
            this.Log(LogType.Warning, itemToStore.name + " is not active and probably shouldn't be stored now at ID "+itemToStore.GetEntityId().ToString());
        }
        itemToStore.gameObject.SetActive(false);
        //itemToStore.OnDisable();
        itemToStore.transform.parent = _itemsPooled;
        //if (itemToStore.Container != null)
            //itemToStore.Container.RequestDrop(itemToStore);
        UpdateActiveList(itemToStore);
        
        //if (reset || spawnItems.Contains(itemToStore))
            //itemToStore.Reset();
        this.Log(LogType.Log, "Despawned " + itemToStore.name + ", will now try to store it at ID "+itemToStore.GetEntityId().ToString());

        TryStore(itemToStore);
    }
    
    public GameObject TrySpawn(GameObject prefab ,Vector3 position = default, Quaternion rotation = default, Transform parent = null)
    {
        if (!prefab.TryGetComponent<Item>(out Item prefabItem))
        {
            this.Log(LogType.Error, prefab.name + " cannot be spawned because it isn't an item");
            
            return null;
        }

        if (!string.IsNullOrEmpty(prefabItem.GetEntityId().ToString()))
        {
            correspondingItems = GetItemsByID(prefabItem.GetEntityId().ToString());

            if (correspondingItems != null)
            {
                foreach (Item item in correspondingItems)
                {
                    TryRemove(item);
                    
                    if (parent == null)
                    {
                        parent = _itemsPooled;
                    }

                    return Spawn(item, position, rotation, parent);
                    
                }
            }

            if (position == default)
            {
                this.Log(LogType.Warning, prefabItem.name + " has no position specified");
                position = new Vector3(0f, 0f, 0f);
            }

            if (rotation == default)
            {
                rotation = new Quaternion(0, 0, 0, 1);
            }
        }
        else
        {
            this.Log(LogType.Warning, prefabItem.name + " has no ID specified");
        }
        GameObject spawned = GameObject.Instantiate(prefab,position, rotation, parent);
        UpdateActiveList(spawned.GetComponent<Item>());
        
        return spawned;
    }
<<<<<<< Updated upstream:MGSC/Assets/Scripts/Game System/ItemManager.cs
    GameObject Spawn(Item item, Vector3 position, Quaternion rotation, Transform parent)
    {
        item.transform.SetParent(parent);
        item.transform.position = position;
        item.transform.rotation = rotation;
        item.gameObject.SetActive(true);

        UpdateActiveList(item);
        
        this.Log(LogType.Log, "Spawned " + item.name);

        return item.gameObject;
    }
    public void Despawn(Item itemToStore, bool reset = false)
    {
        if (itemToStore.gameObject == null)
        {
            this.Log(LogType.Error, itemToStore.name + " gameobject not found, probably destroyed ?");
            return;
        }
        if (!itemToStore.gameObject.activeSelf)
        {
            this.Log(LogType.Warning, itemToStore.name + " is not active and probably shouldn't be stored now at ID "+itemToStore.GetEntityId().ToString());
        }
        itemToStore.gameObject.SetActive(false);
        //itemToStore.OnDisable();
        itemToStore.transform.parent = _itemsPooled;
        //if (itemToStore.Container != null)
            //itemToStore.Container.RequestDrop(itemToStore);
        UpdateActiveList(itemToStore);
        
        //if (reset || spawnItems.Contains(itemToStore))
            //itemToStore.Reset();
        this.Log(LogType.Log, "Despawned " + itemToStore.name + ", will now try to store it at ID "+itemToStore.GetEntityId().ToString());

        TryStore(itemToStore);
    }
    public void StoreCopy(Item item)
    {
        if (spawnItems.Contains(item))
        {
            spawnItems.Remove(item);
            GameObject copy = GameObject.Instantiate(item.gameObject, item.transform.position, item.transform.rotation, _itemsPooled);
            
            Item itemCopy = copy.GetComponent<Item>();
            Despawn(itemCopy);
            
            spawnItems.Add(itemCopy);
        }
        else
        {
            Despawn(GameObject.Instantiate(item, item.transform.position, item.transform.rotation, _itemsPooled), true);
        }
    }

    public void StoreAllActive()
    {
        //Debug.Log("Active items : " + activeItems.Count + " (being stored)");
        for (var index = activeItems.Count-1; index >= 0; index--)
        {
            var item = activeItems[index];
            Despawn(item);
        }
    }

    public void SpawnInitialItems(int shiftCount)
    {
        this.Log("Spawning initial items");
        int i = 0;
        foreach (var item in spawnItems)
        {
            if (_maxSpawnItemsPerShift.ContainsKey(shiftCount))
                if (_maxSpawnItemsPerShift[shiftCount] <= i)
                    break;
            this.Log(item.transform.position);
            int index = spawnItems.IndexOf(item);
            TrySpawn(item.gameObject, spawnPositions[index], item.transform.rotation, item.transform.parent);
            i++;
        }
    }

    public void SpawnInitialItems()
    {
        SpawnInitialItems(1000);
    }
=======
    
    */
    #endregion
}
