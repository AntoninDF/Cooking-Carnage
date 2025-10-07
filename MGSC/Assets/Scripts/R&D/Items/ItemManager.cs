using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemManager : MonoBehaviour
{
    public Interactor Interactor { get => interactor; set => interactor = value; }
    [SerializeField] private Interactor interactor;
    public Inventory Inventory { get => inventory; set => inventory = value; }
    [SerializeField] private Inventory inventory;

    private List<Item> items;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (interactor == null) interactor = FindAnyObjectByType<Interactor>();
        if (inventory == null) inventory = FindAnyObjectByType<Inventory>();

        if (interactor == null || inventory == null)
        {
            Debug.LogError("[ItemManager] Start : No interactor or inventory found. Please assign both in the inspector.");
        }

        items = FindObjectsByType<Item>(FindObjectsSortMode.None).ToList();
        foreach (Item item in items)
        {
            item.Manager = this;
            interactor.OnTryInteract += item.TryInteract;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
