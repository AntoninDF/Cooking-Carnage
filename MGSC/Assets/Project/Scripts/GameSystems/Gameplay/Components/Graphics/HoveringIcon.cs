using System;
using System.Collections.Generic;
using UnityEngine;

public class HoveringIcon : MonoBehaviour
{

    #region Static part
    /// <summary>
    /// IconCategory is setup to work with enum flags. It can have 32 members at most
    /// </summary>
    public enum IconCategory
    {
        None =                          0,
        ________Pointers________ =      1 << 28, // "Folders" start from 28
        IngredientPointer =             1 << 0,
        PlatePointer =                  1 << 1,
        RecipePointer =                 1 << 2,
        BlenderPointer =                1 << 3,
        OvenPointer =                   1 << 4,
        ShiftStartTriggerPointer =      1 << 5,
        ServingAreaPointer =            1 << 6,
        KillButtonPointer =             1 << 7,
        SlushMachinePointer =           1 << 8,
        OrderTV =                       1 << 9,
        ________Warnings________ =      1 << 29,
        OvenWarning =                   1 << 14,
        SlushWarning =                  1 << 15,
    }
    private static List<HoveringIcon> _icons = new();

    public static void DisplayObjectsOfType(IconCategory category)
    {
        foreach (var obj in _icons)
        {
            if (obj._category == category)
                obj.Display();
        }
    }
    public static void HideObjectsOfType(IconCategory category)
    {
        foreach (var obj in _icons)
            if (obj._category == category)
                obj.Hide();
    }
    public static void DisplayAll()
    {
        foreach (var obj in _icons)
            obj.Display();
    }
    public static void HideAll()
    {
        foreach (var obj in _icons)
            obj.Hide();
    }

    #endregion



    [SerializeField] private IconCategory _category; 
    [SerializeField] private Transform _animatedObject;
    [SerializeField] private bool _controlAnimatedObject;
    [SerializeField] private bool _faceCamera = true;
    [SerializeField] private float _hoveringDuration = 3;
    [SerializeField] private float _hoveringRange = 0.3f;

    Vector3 _defaultPosition;

    private void Update()
    {
        if (!_controlAnimatedObject || _animatedObject == null)
            return;

        _animatedObject.localPosition =
            _defaultPosition -
            Quaternion.Inverse(_animatedObject.parent.rotation) *  _animatedObject.forward * (_hoveringRange * (Mathf.Sin(Time.time * 6.28f / (_hoveringDuration == 0f ? 0.0001f : _hoveringDuration)) * 0.5f + 0.5f));

        if (Camera.main != null && _faceCamera)
            _animatedObject.rotation = Quaternion.LookRotation(_animatedObject.transform.forward, Camera.main.transform.position - _animatedObject.position);
    }
    private void Start()
    {
        if (_animatedObject == null)
        {
            Debug.LogWarning("[HoveringIcon] The icon '" + name + 
                "' was disabled because it didn't have a reference to an animated object");
            this.enabled = false;
        }
        _defaultPosition = _animatedObject.localPosition;
        Hide();
    }
    private void Display()
    {
        _animatedObject?.gameObject.SetActive(true);
    }
    private void Hide()
    {
        _animatedObject?.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (!_icons.Contains(this))
            _icons.Add(this);
    }
    private void OnDisable()
    {
        _icons.Remove(this);
    }
}
