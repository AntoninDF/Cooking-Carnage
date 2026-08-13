using System.Collections.Generic;
using UnityEngine;

public class ItemManager : IManager
{
    #region Fields    
    List<Item> _activeItems = new List<Item>();
    
    private Transform _itemsPooled;
    
    
    
    
    #endregion
    
    public ItemManager()
    {
        Register();
        Item.OnItemEnabled += AddItem;
        Item.OnItemDisabled += RemoveItem;

        GameLoop.OnReset += Reset;
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

    void AddItem(Item item)
    {
        if (!_activeItems.Contains(item))
            _activeItems.Add(item);
    }
    void RemoveItem(Item item) => _activeItems.Remove(item);
    

    private void UpdateActiveList(Item item)
    {
        if (item.gameObject != null)
        {
            if (item.gameObject.activeSelf)
            {
                if (!_activeItems.Contains(item))
                {
                    _activeItems.Add(item);
                    this.Log(LogType.Log, item.name + " has been registered as active item");
                }
            }
            else
            {
                if (_activeItems.Contains(item))
                {
                    _activeItems.Remove(item);
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
        
        // UpdateActiveList(spawned.GetComponent<Item>());
        
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

        //UpdateActiveList(itemToDespawn);
        
        this.Log(LogType.Log, "Despawned " + itemToDespawn.name );

        if (itemToDespawn.Grabbable.Container != null)
        {
            itemToDespawn.Grabbable.Container.RequestDrop(itemToDespawn.Grabbable);
        }
        
        GameObject.Destroy(itemToDespawn.gameObject);
        
    }
    
    public void Reset()
    {
        DestroyAllActive();
    }
    public void DestroyAllActive()
    {
        for (var index = _activeItems.Count-1; index >= 0; index--)
        {
            var item = _activeItems[index];
            GameObject.Destroy(item.gameObject);
        }
        _activeItems.Clear();
    }

}
