using UnityEngine;

public class Inventory : MonoBehaviour
{

    public IGrabbable HeldItem {  get; private set; }
    public Interactor Interactor { get => interactor; set => interactor = value; }
    [SerializeField] private Interactor interactor;

    [SerializeField] private Transform handSlot;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (interactor == null) interactor = FindAnyObjectByType<Interactor>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGrabbedItemTransform();
    }

    public bool Grab<T>(T item) where T : IGrabbable
    {
        if (HeldItem != null) return false;
        HeldItem = item;
        interactor.OnTryThrow += HeldItem.TryThrow;
        return true;
    }

    public IGrabbable Release()
    {
        interactor.OnTryThrow -= HeldItem.TryThrow;
        IGrabbable temp = HeldItem;
        HeldItem = null;
        return temp;
    }

    void UpdateGrabbedItemTransform()
    {
        if (HeldItem == null) return;
        if (handSlot == null)
        {
            Debug.LogError("[Inventory] UpdateGrabbedItemTransform : Please assign a hand slot in the inspector");
        }
        HeldItem.Transform.position = handSlot.position;
        HeldItem.Transform.rotation = handSlot.rotation;
    }
}
