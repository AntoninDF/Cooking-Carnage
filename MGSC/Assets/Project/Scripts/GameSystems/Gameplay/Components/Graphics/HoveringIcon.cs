using System;
using System.Collections.Generic;
using UnityEngine;

public class HoveringIcon : MonoBehaviour
{

    #region Static part
    public enum IconCategory
    {
        None =                          0, // None has to be 0
        ________Pointers________ =      1000, // "Folders" start from 1000
        IngredientPointer =             1, // Items of a folder are layed out hundred by hundred (10 folders max)
        PlatePointer =                  2,
        RecipePointer =                 3,
        BlenderPointer =                4,
        OvenPointer =                   5,
        ShiftStartTriggerPointer =      6,
        ServingAreaPointer =            7,
        KillButtonPointer =             8,
        SlushMachinePointer =           9,
        ________Warnings________ =      1001,
        OvenWarning =                   101
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
    [SerializeField] private float _hoveringDuration = 3;
    [SerializeField] private float _hoveringRange = 0.3f;

    Vector3 _defaultPosition;

    private void Update()
    {
        if (!_controlAnimatedObject || _animatedObject == null)
            return;

        _animatedObject.position =
            _defaultPosition -
            _animatedObject.forward * _hoveringRange * (Mathf.Sin(Time.time * 6.28f / _hoveringDuration) * 0.5f + 0.5f);
    }
    private void Start()
    {
        if (_animatedObject == null)
        {
            Debug.LogWarning("[HoveringIcon] The icon '" + name + 
                "' was disabled because it didn't have a reference to an animated object");
            this.enabled = false;
        }
        _defaultPosition = _animatedObject.position;
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
