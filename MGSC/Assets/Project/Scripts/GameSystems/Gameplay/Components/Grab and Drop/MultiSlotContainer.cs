using UnityEngine;
using Features;
using System.Collections.Generic;
using System.Linq;
public class MultiSlotContainer : MonoBehaviour, IFeatureCollection
{
    #region Properties
    public FeatureCollection Collection { get; set; } = new();
    public Features.F_MultiSlotContainer Container { get; set; }
    public F_Interactable Interactable{ get; set; }
    public List<F_Grabbable> HeldItems => Container.SoloContainers.Select(s => s.HeldItem).ToList();
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
    private void TryLockItem(F_Grabbable item)
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
        else
        {
            foreach (GameObject slot in slots)
            {
                if (!slot.TryGetComponent<SlotContainer>(out var container)) continue;
                _soloContainers.Add(container.Container);
            }
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
