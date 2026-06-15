using UnityEngine;
using Features;
using System;

/// <summary>
/// The slot container can receive up to one IGrabbable object
/// </summary>
public class SlotContainer : MonoBehaviour, IFeatureCollection
{
    public FeatureCollection Collection { get; set; } = new();

    public Grabbable HeldItem => Container.HeldItem;
    public F_SoloContainer Container { get; set; }
    public Interactable Interactable { get; set; }

    public bool IsInteractable
    {
        get => Interactable.Enabled;
        set => Interactable.Enabled = value;
    }

    #region Fields
    [SerializeField] private bool isInteractable = true;
    [Tooltip("If the slot is passthrough, it will allow the player to grab the item by clicking " +
        "on the slot's collider as well as the ingredient, instead of just the ingredient")]
    [SerializeField] private bool passThrough = true;
    #endregion

    void Awake()
    {
        Container = new(this);
        Interactable = new(this);
        BindEvents();
        IsInteractable = isInteractable;
    }
    void Reset()
    {
        if (isInteractable && !GetComponent<Collider>())
        {
            var col = gameObject.AddComponent<SphereCollider>();
            col.radius = 0.1f;
        }
        else if (!isInteractable && TryGetComponent(out Collider col))
        {
            col.enabled = false;
        }
    }

    private void BindEvents()
    {
        Interactable.OnInteraction += OnInteract;
    }
    private void OnInteract(InteractionContext context)
    {
        if (context.TryGetFeature(out F_Container container))
        {
            bool hasGrabbed = Container.RequestGrab(container.HeldItem);
            if (!hasGrabbed && passThrough)
            {
                container.RequestGrab(Container.HeldItem);
            }
        }
    }
}