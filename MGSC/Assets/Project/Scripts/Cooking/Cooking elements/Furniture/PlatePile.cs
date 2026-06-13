using Controller;
using Cooking;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

// Please don't inherit from this class, it isn't meant to
[RequireComponent(typeof(ObjectPile))]
[RequireComponent(typeof(CapsuleCollider))]
[ExecuteAlways]
public sealed class PlatePile : MonoBehaviour, IMultiContainer, ICallbackBroadcaster
{
    #region Properties


    public List<IGrabbable> HeldItems { get; set; } = new();

    public UnityEvent<IGrabbable> OnGrab { get; set; }

    public UnityEvent<IGrabbable> OnDrop { get; set; }

    public bool IsLocked { get; set; }

    public bool Passthrough => true;

    public IGrabbable HeldItem => HeldItems.Count > 0 ? HeldItems[^1] : null;

    public PlayerInterface Player { get; set; }
    public bool EnableCallbacks { get; set; }

    public ICallbackBroadcaster Self => this;
    #endregion




    #region Fields
    [SerializeField] private Plate _platePrefab;

    // Private fields

    private ObjectPile _pile;
    private ItemManager _itemManager;
    private CapsuleCollider _capsuleCollider;

    #endregion

    #region Methods

    private void Start()
    {
        Reference.GetManager(out  _itemManager);
        Setup();
    }

    public void Setup()
    {
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _pile = GetComponent<ObjectPile>();
        _pile.Prefabs = new();
        _pile.Prefabs.Add(_platePrefab.gameObject);
        _pile.TryRegenerate();
        UpdateCollider();

        HeldItems.Clear();

        foreach (Plate plate in _pile.GetComponentsInChildren<Plate>(true))
        {
            Debug.Log("Grabbed a plate named " + plate.name);
            RequestGrab(plate);
        }

        //SetPlateComponentsEnabled(false);
    }

    public void SetPlateComponentsEnabled(bool enabled)
    {
        foreach(var plate in transform.GetComponentsInChildren<Plate>(true))
        {
            plate.enabled = enabled;
        }
    }

    public void UpdateCollider()
    {
        _capsuleCollider.height = _pile.Height;
        _capsuleCollider.center  = Vector3.up * _pile.Height/2;
    }

    public bool RequestGrab(IGrabbable item)
    {
        return TryGrab(item);
    }

    public bool RequestDrop(IGrabbable item = null)
    {
        return TryDrop(item);
    }

    public void Empty()
    {
        foreach(var plate in GetComponentsInChildren<Plate>(true))
        {
            _itemManager.Despawn(plate);
        }
        HeldItems.Clear();
        _pile.TargetObjectCount = 0;
        _pile.TryRegenerate();
    }

    public bool CanGrab(IGrabbable item)
    {
        if (IsLocked) return false;
        if (item == null) return false;
        if (item.Container != null) return false; // Can only grab the object if it has just been spawned, and doesn't come from the player or any other container
        if (item.IsLocked) return false;

        return true;
    }

    public bool CanDrop(IGrabbable item = null)
    {
        if (item == null) return false;
        if (HeldItem != item) return false;
        if (IsLocked) return false;
        if (item.IsLocked) return false;
        if (item is not Plate) return false;

        return true;
    }
    private bool TryGrab(IGrabbable item)
    {
        if (!CanGrab(item))
            return false;
        Grab(item);
        return true;
    }

    private bool TryDrop(IGrabbable item = null)
    {
        if (!CanDrop(item))
            return false;
        Drop(item);
        return true;
    }
    private void Grab(IGrabbable item)
    {
        HeldItems.Add(item);
        Self.Callback(OnGrab, item); // There isn't much reason to have this callback, but why not...
        Plate plate = item as Plate;
        plate.Container = this;
        plate.Collider.enabled = false;
        plate.transform.SetParent(this.transform);
        UpdateCollider();
    }
    private void Drop(IGrabbable item)
    {
        Debug.Log("PlatePile drops an item");
        HeldItems.Remove(item);
        Plate plate = item as Plate;
        plate.transform.SetParent(null);
        plate.Collider.enabled = true;
        _pile.TargetObjectCount = _pile.transform.childCount;
        //_pile.TryRegenerate();
        UpdateCollider();
    }

    #endregion
}

#if UNITY_EDITOR

[CustomEditor(typeof(PlatePile))]
public class PlatePileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PlatePile pile = (PlatePile)target;
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
        {
            pile.Setup();
        }
    }
}


#endif