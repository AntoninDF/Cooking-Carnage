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

    private List<Item> containedItems = new();
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
        containedItems.Add(item.Manager.Inventory.HeldItem as Item);
        var temp = item.Manager.Inventory.Release();
        item.Manager.Interactor.OnTryInteract -= ((Item)temp).TryInteract;
        temp.Transform.gameObject.SetActive(false);
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
        foreach (SO_Item item in dataModel.itemsNeeded)
        {
            int a = containedItems.Count((elm) => (elm.dataModel == item));
            int b = dataModel.itemsNeeded.Count((elm) => elm == item);
            Debug.Log("[Recipe] CheckRecipe : " + item + " : " + a + " contained, " + b + " needed.");
            if (a < b)  return false;
        }
        return true;
    }
}
