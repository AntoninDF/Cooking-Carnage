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
    public SoloContainer Container { get; set; }
    public Interactable Interactable { get; set; }

    public bool IsInteractable
    {
        get => Interactable.Enabled;
        set => Interactable.Enabled = value;
    }

    #region Fields
    [SerializeField] private bool isInteractable = true;
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
        if (context.TryGetFeature(out Container container))
        {
            Container.RequestGrab(container.HeldItem);
        }
    }
}