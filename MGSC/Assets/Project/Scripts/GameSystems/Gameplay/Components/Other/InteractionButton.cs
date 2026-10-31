using Features;
using System;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractionButton : MonoBehaviour, IFeatureCollection
{
    public FeatureCollection Collection { get; set; } = new();
    public F_Interactable Interactable;
    public F_LookAtTarget LookAtTarget;
    public Action<InteractionContext> OnInteraction { get => Interactable.OnInteraction; set => Interactable.OnInteraction = value; }
    public ModelController Model;

    private bool _initialized = false;
    void Awake()
    {
        Interactable = new(this);
        LookAtTarget = new(this, transform);
    }
    void Start()
    {
        BindEvents();
        _initialized = true;
        SetupMeshCollider();
    }
    private void OnEnable() => SetupMeshCollider();
    private void BindEvents()
    {
        if (Model != null)
        {
            LookAtTarget.OnStartLookAt += Model.Outline.EnableOutline;
            LookAtTarget.OnStopLookAt += Model.Outline.DisableOutline;
        }
    }

    protected void SetupMeshCollider()
    {
        if (!_initialized) return;
        if (Model == null) return;

        MeshCollider col;
        if (!TryGetComponent(out col))
        {
            foreach (var collider in transform.GetComponents<Collider>())
            {
                collider.enabled = false;
            }
            col = transform.AddComponent<MeshCollider>();
        }
        col.sharedMesh = Model.GetCombinedMeshes(transform);
    }
}
