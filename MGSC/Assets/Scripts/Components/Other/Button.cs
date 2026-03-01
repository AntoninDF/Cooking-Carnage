using Features;
using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Button : MonoBehaviour, IFeatureCollection
{
    public FeatureCollection Collection { get; set; } = new();
    public Interactable Interactable;
    public Action<InteractionContext> OnInteraction => Interactable.OnInteraction;

    void Awake()
    {
        Interactable = new(this);
    }
}
