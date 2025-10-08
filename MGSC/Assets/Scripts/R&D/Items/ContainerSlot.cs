using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ContainerSlot : MonoBehaviour
{
    public IGrabbable HeldItem {  get; private set; }
    public Collider Collider { get; private set; }

    [SerializeField] private bool checkOverlap;
    [SerializeField] private Material available;
    [SerializeField] private Material unavailable;

    private void Start()
    {
        Collider = GetComponent<Collider>();
    }

    private void Update()
    {
        UpdateMaterial();
    }
    public void Grab(IGrabbable item)
    {
        UpdateMaterial();
        if (!IsFree()) return;
        if (item is not Item) return;
        ((Item)item).SetPosition(this.transform.position);
        HeldItem = item;
        UpdateMaterial();
    }
    public void Release()
    {
        HeldItem = null;
        UpdateMaterial();
    }

    public bool IsFree()
    {
        bool isFree = HeldItem == null;

        if (checkOverlap)
        {
            List<Collider> colliders = Physics.OverlapBox(transform.position, transform.localScale / 2).ToList();
            if (colliders.Any((col) => col.GetComponent<Item>() != null)) isFree = false;
        }

        return isFree;
    }

    private void UpdateMaterial()
    {
        // Update the material to show availability
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null && available != null && unavailable != null)
        {
            if (IsFree())
                renderer.material = available;
            else
                renderer.material = unavailable;
        }
    }
}
