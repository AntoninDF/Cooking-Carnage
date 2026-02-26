using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerActor : FeatureComponent, IFeatureCollection
{

    public FeatureCollection Collection { get; set; } = new();

    public List<FeatureComponent> AvailableFeatures = new();

    public float ReachDistance { get; set; } = 10f;
    public Vector3 LookPosition => _scanner.LastScanHitPosition;
    public bool SortFeaturesByDistance { get => _scanner.SortByDistance; set => _scanner.SortByDistance = value; }

    #region Fields

    private FeatureScannerByRaycast _scanner = new ();
    private SoloContainer _container;
    private Dictionary<PlayerActionType, Dictionary<System.Type, List<Func<FeatureComponent, bool>>>> _playerActions = new();
    private Dictionary<System.Type, List<Func<FeatureComponent, bool>>> _constantActions = new();

    #endregion


    #region Methods


    public PlayerActor(IFeatureCollection owner) : base(owner)
    {
    }

    public override void Init()
    {
        _container = new(this);
    }

    public override void Update()
    {
        Collection.Update();
        GetLookedAtObjects();
        UpdateConstantActions();
    }

    private void GetLookedAtObjects()
    {
        SetupFeatureScanner();
        AvailableFeatures = _scanner.Get();
    }

    private void SetupFeatureScanner()
    {
        var cam = Camera.main;
        _scanner.Set(cam.transform.position, cam.transform.forward, ReachDistance);
    }

    public bool TryAnyPlayerAction()
    {
        if (AvailableFeatures == null || AvailableFeatures.Count == 0) return false;
        bool result = false;
        foreach (FeatureComponent feature in AvailableFeatures)
        {
            if (TryPlayerAction(feature))
            {
                result = true;
            }
        }

        return result;
    }

    private bool TryPlayerAction(FeatureComponent feature)
    {
        bool primaryActionPerformed = false;
        if (_playerActions.TryGetValue(PlayerActionType.Primary, out var primaryActionsByType)) {
            if (primaryActionsByType.TryGetValue(feature.GetType(), out var actions)) {
                foreach (var action in actions) {
                    if (action(feature)) {
                        primaryActionPerformed = true;
                        break;
                    }
                }
            }
        }

        if (_playerActions.TryGetValue(PlayerActionType.Secondary, out var secondaryActionsByType)) {
            if (secondaryActionsByType.TryGetValue(feature.GetType(), out var actions)) {
                foreach (var action in actions) {
                    action(feature);
                }
            }
        }
        if (_playerActions.TryGetValue(PlayerActionType.Default, out var defaultActionsByType)) {
            if (secondaryActionsByType.TryGetValue(feature.GetType(), out var actions)) {
                foreach (var action in actions) {
                    action(feature);
                }
            }
        }
        

        return primaryActionPerformed;
    }

    public void AddPlayerAction<T>(PlayerActionType mode, Func<FeatureComponent, bool> action) where T : FeatureComponent => AddPlayerAction(typeof(T), mode, action);
    /// <summary>
    /// Player actions are only triggered externally by TryPlayerAction() or TryAnyPlayerAction()
    /// </summary>
    /// <param name="type"></param>
    /// <param name="action"></param>
    public void AddPlayerAction(System.Type type,  PlayerActionType mode, Func<FeatureComponent, bool> action)
    {
        var key = type;
        if (!_playerActions.ContainsKey(mode))
        {
            _playerActions[mode] = new();
        }
        if (!_playerActions[mode].ContainsKey(key))
        {
            _playerActions[mode][key] = new();
        }
        if (!_playerActions[mode][key].Contains(action))
            _playerActions[mode][key].Add(action);
    }
    public void AddConstantAction<T>(Func<FeatureComponent, bool> action) where T : FeatureComponent => AddConstantAction(typeof(T), action);
    /// <summary>
    /// Constant actions are executed every frame.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="action"></param>
    public void AddConstantAction(System.Type type,  Func<FeatureComponent, bool> action)
    {
        var key = type;
        if (!_constantActions.ContainsKey(key))
        {
            _constantActions[key] = new();
        }
        if (!_constantActions[key].Contains(action))
            _constantActions[key].Add(action);
    }

    private void UpdateConstantActions()
    {
        if (AvailableFeatures == null || AvailableFeatures.Count == 0) return;

        foreach (FeatureComponent feature in AvailableFeatures)
        {
            foreach (System.Type type in _constantActions.Keys)
            {
                if (type.IsAssignableFrom(feature.GetType()))
                {
                    foreach (var action in _constantActions[type])
                    {
                        action(feature);
                    }
                }
            }
        }
    }
    #endregion
}

public enum PlayerActionType
{
    Default = 0, // Default should fall back to secondary
    Primary = 1, // Only one primary action can be executed at a time.
                 // If several primary actions are requested at the same time,
                 // the first in queue will be treated, and others should be discarded
    Secondary = 2, // Secondary actions are always performed as long as their requirements are met
}