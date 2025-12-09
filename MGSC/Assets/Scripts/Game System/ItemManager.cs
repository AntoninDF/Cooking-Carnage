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
        foreach (Item item in ReferenceManager.Instance.GetElementsOfType<Item>())
        {
            if (!_itemsByID.ContainsKey(item.ID))
            {
                _itemsByID.Add(item.ID, new List<Item>());
            }
        }
        
    }

    private void TryStore(Item item)
    {
        if (item == null)
        {
            return;
        }
        
        if (!_itemsByID.ContainsKey(item.ID))
        {
            _itemsByID.Add(item.ID, new List<Item>());
        }
        else
        {
            if (!_itemsByID[item.ID].Contains(item))
            {
                _itemsByID[item.ID].Add(item);
            }
        }
    }

    private void TryRemove(Item item)
    {
        if (item == null)
        {
            return;
        }

        if (_itemsByID.ContainsKey(item.ID))
        {
            if (_itemsByID[item.ID].Contains(item))
            {
                _itemsByID[item.ID].Remove(item);
            }
        }
        else
        {
            this.Log(LogType.Error, item.name +" ID probably changed, should not be possible");
        }
    }

    List<Item> GetItemsByID(string id)
    {
        if (_itemsByID.ContainsKey(id))
        {
            return _itemsByID[id];
        }
        
        return null;
    }

    private List<Item> correspondingItems;
    public GameObject TrySpawn(GameObject prefab ,Vector3 position = default, Quaternion rotation = default, Transform parent = null)
    {
        Item prefabItem = prefab.GetComponent<Item>();
        
        if (prefabItem == null)
        {
            this.Log(LogType.Error, prefab.name + " cannot be spawned because it isn't an item");
        }
        
        correspondingItems.Clear();
        correspondingItems = GetItemsByID(prefabItem.ID);

        if (correspondingItems != null)
        {
            foreach (Item item in correspondingItems)
            {
                if (true)
                {
                    // Ajouter conditions si besoins
                    TryRemove(item);
                    
                    return Spawn(item, position, rotation, parent);
                }
            }
        }

        if (position == default)
        {
            this.Log(LogType.Error, prefabItem.name + " has no position to be instantiated");
            return null;
        }

        if (rotation == default)
        {
            rotation =new Quaternion(0,0,0,1);
        }
        
        return GameObject.Instantiate(prefab,position, rotation, parent);
    }
    GameObject Spawn(Item item, Vector3 position, Quaternion rotation, Transform parent)
    {
        item.transform.SetParent(parent);
        item.transform.position = position;
        item.transform.rotation = rotation;
        item.gameObject.SetActive(true);
        return item.gameObject;
    }
    
    public void Despawn(Item itemToStore)
    {
        if (itemToStore.gameObject == null)
        {
            this.Log(LogType.Error, itemToStore.name + " gameobject not found, probably destroyed ?");
            return;
        }

        if (!itemToStore.gameObject.activeSelf)
        {
            this.Log(LogType.Warning, itemToStore.name + " is not active and probably shouldn't be stored now");
        }
        
        itemToStore.gameObject.SetActive(false);
        itemToStore.Reset();

        TryStore(itemToStore);
    }
}
