using UnityEngine;
using Features;

/// <summary>
/// The slot container can receive up to one IGrabbable object
/// </summary>
[RequireComponent(typeof(Collider))]
public class SlotContainer : MonoBehaviour, IFeatureCollection
{
    public FeatureCollection Collection { get; set; } = new();

    public Grabbable HeldItem => Container.HeldItem;
    public SoloContainer Container { get; set; }


    void Start()
    {
        Container = new(this);
    }
}