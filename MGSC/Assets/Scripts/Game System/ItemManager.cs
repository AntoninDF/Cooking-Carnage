using System.Collections.Generic;
using Cooking;
using GameSystem;
using UnityEngine;

public class ItemManager : IManager
{
    #region Properties
    
    #endregion

    #region Fields
    
    Dictionary<string, List<Item>> _itemsByID = new Dictionary<string, List<Item>>();
    
    private Transform _itemsPooled;
    
    
    #endregion
    
    public ItemManager()
    {
        Register();
    }
    
    public void Register()
    {
        ReferenceManager.Instance.SetManager(this);
    }

    public void Unregister()
    {
        ReferenceManager.Instance.RemoveManager(this);
    }

    public void Init()
    {
        GameObject newRoot = new GameObject("Items Pool");
        _itemsPooled = newRoot.transform;
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
            if (item.ID != null)
            {
                id = item.ID;
            }
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

    private void TryRemove(Item item, string id=null)
    {
        if (item == null)
        {
            this.Log(LogType.Error, item.name + " can't be removed because it is null");
            return;
        }

        if (id == null)
        {
            id = item.ID;
        }

        if (_itemsByID.ContainsKey(id))
        {
            if (_itemsByID[id].Contains(item))
            {
                _itemsByID[id].Remove(item);
                this.Log(LogType.Log, item.name +" has been removed, there is "+ _itemsByID[item.ID].Count+" left");
            }
            else
            {
                this.Log(LogType.Warning, item.name + " can't be removed because it wasn't stored at ID "+item.ID);
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

    private List<Item> correspondingItems;
    public GameObject TrySpawn(GameObject prefab ,Vector3 position = default, Quaternion rotation = default, Transform parent = null)
    {
        
        if (!prefab.TryGetComponent<Item>(out Item prefabItem))
        {
            this.Log(LogType.Error, prefab.name + " cannot be spawned because it isn't an item");
            
            return null;
        }

        if (prefabItem.ID != "")
        {
            correspondingItems = GetItemsByID(prefabItem.ID);

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

        return GameObject.Instantiate(prefab,position, rotation, parent);
    }
    GameObject Spawn(Item item, Vector3 position, Quaternion rotation, Transform parent)
    {
        item.transform.SetParent(parent);
        item.transform.position = position;
        item.transform.rotation = rotation;
        item.gameObject.SetActive(true);
        
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
            this.Log(LogType.Warning, itemToStore.name + " is not active and probably shouldn't be stored now at ID "+itemToStore.ID);
        }
        
        itemToStore.gameObject.SetActive(false);
        if (reset)
            itemToStore.Reset();
        
        this.Log(LogType.Log, "Despawned " + itemToStore.name + ", will now try to store it at ID "+itemToStore.ID);

        TryStore(itemToStore);
    }


    public void StoreCopy(Item item)
    {
        Despawn(GameObject.Instantiate(item,item.transform.position, item.transform.rotation, null),true);
    }
}
