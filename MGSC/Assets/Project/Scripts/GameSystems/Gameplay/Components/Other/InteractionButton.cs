using Features;
using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractionButton : MonoBehaviour, IFeatureCollection
{
    public FeatureCollection Collection { get; set; } = new();
    public F_Interactable Interactable;
    public Action<InteractionContext> OnInteraction => Interactable.OnInteraction;

    void Awake()
    {
        Interactable = new(this);
    }
}
