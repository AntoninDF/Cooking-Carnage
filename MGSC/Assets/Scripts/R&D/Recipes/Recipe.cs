using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Item))]
public class Recipe : MonoBehaviour, IInteractable
{
    public Action OnInteract {  get; private set; }

    [Tooltip("Select the SO_Recipe object that models how this recipe should be/behave.")]
    [SerializeField] private SO_Recipe dataModel;

    private Dictionary<SO_Item, int> containedItems = new() ;
    private Item item;
    bool initialized = false;

    void Start()
    {
        item = GetComponent<Item>();
        
    }

    void Update()
    {
        if ( !initialized) 
        {
            item.Manager.Interactor.OnTryInteract += TryInteract;
            initialized = true;
        }
    }

    public void Interact(Interactor interactor)
    {
        SO_Item itemType = ((Item)item.Manager.Inventory.HeldItem).dataModel;
        if (!containedItems.ContainsKey(itemType)) containedItems[itemType] = 0;
        containedItems[itemType]++;
        var temp = item.Manager.Inventory.Release();
        GameObject.Destroy(temp.Transform.gameObject);
        Debug.Log("[Recipe] Interact : Recipe currently contains : " + containedItems.Count);
        if (CheckRecipe())
        {
            Debug.Log("[Recipe] Interact : The recipe was valid, trying to change into the full meal");
            GameObject.Instantiate(dataModel.itemPrefab, this.transform.position, this.transform.rotation);
        }
        
    }

    public void TryInteract(Interactor interactor)
    {
        if (item.Manager.Inventory.HeldItem == null) return;
        if (!dataModel.itemsNeeded.Contains(((Item)item.Manager.Inventory.HeldItem).dataModel)) return;
        if (interactor.LookInfo.collider.gameObject != this.gameObject) return;
        Interact(interactor);

    }
    private bool CheckRecipe()
    {
        string containedItemsString = "[Recipe] CheckRecipe : Contained items\n";
        foreach (KeyValuePair<SO_Item, int> kvp in containedItems)
        {
            containedItemsString += kvp.Key.ToString();
            containedItemsString += " : ";
            containedItemsString += kvp.Value.ToString();
        }
        Debug.Log(containedItemsString);

        foreach (SO_Item item in dataModel.itemsNeeded)
        {
            if (!containedItems.ContainsKey(item)) return false;
            int a = containedItems[item];
            int b = dataModel.itemsNeeded.Count((elm) => elm == item);
            Debug.Log("[Recipe] CheckRecipe : " + item + " : " + a + " contained, " + b + " needed.");
            if (a < b)  return false;
        }
        return true;
    }
}
