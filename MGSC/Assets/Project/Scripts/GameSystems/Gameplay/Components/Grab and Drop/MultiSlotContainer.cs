using UnityEngine;
using Features;
using System.Collections.Generic;
public class MultiSlotContainer : MonoBehaviour, IFeatureCollection
{
    #region Properties
    public FeatureCollection Collection { get; set; } = new();
    public Features.F_MultiSlotContainer Container { get; set; }
    public Interactable Interactable{ get; set; }
    #endregion

    #region Fields
    [SerializeField] private List<GameObject> slots;
    [SerializeField] private bool detectSlots = true;
    [SerializeField] private bool lockItemsWhenAdded = false;

    List<F_SoloContainer> _soloContainers = new();
    #endregion

    #region Methods
    private void Awake()
    {
        Container = new(this);
        Interactable = new(this);
    }
    private void Start()
    {
        EnforceSlotList();

        Collection.LateSetup();
        BindEvents();
        Collection.LateSetup();
    }
    private void Update()
    {
        Collection.Update();
    }

    private void BindEvents()
    {
        Interactable.OnInteraction += OnInteract;
        Container.OnGrab += TryLockItem;
    }

    private void OnInteract(InteractionContext context)
    {
        if (context.TryGetFeature(out F_Container container))
        {
            Container.RequestGrab(container.HeldItem);
        }
    }
    private void TryLockItem(Grabbable item)
    {
        if (lockItemsWhenAdded)
            item.IsLocked = true;
    }
    private void EnforceSlotList()
    {
        if (slots == null) slots = new ();
        if (slots.Count == 0)
        {
            if (detectSlots)
            {
                _soloContainers = FeatureScannerByHierarchy.Get<F_SoloContainer>(gameObject);
            }
            else
                Debug.LogWarning("No slots were added or detected on MultiSlotContainer '" + name + "'");
        }
        foreach (var obj in _soloContainers)
        {
            var transform = obj.GetParentTransform();
            if (transform != null) slots.Add(transform.gameObject);
            else Debug.LogWarning("Slot transform is null");

            Container.TryAddSlot(obj);
        }
    }
    #endregion
}
