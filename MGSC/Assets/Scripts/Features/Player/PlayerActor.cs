using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerActor : FeatureComponent, IFeatureCollection
{

    public FeatureCollection Collection { get; set; } = new();

    public List<FeatureComponent> AvailableFeatures = new();

    public float ReachDistance { get; set; } = 10f;

    #region Fields

    private FeatureScannerByRaycast _scanner = new ();
    private SoloContainer _container;
    private Dictionary<System.Type, List<Func<FeatureComponent, bool>>> _playerActions = new();
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

        foreach (FeatureComponent feature in AvailableFeatures)
        {
            TryPlayerAction(feature);
        }

        return true;
    }

    private bool TryPlayerAction(FeatureComponent feature)
    {
        foreach (System.Type type in _playerActions.Keys)
        {
            if (type.IsAssignableFrom(feature.GetType()))
            {
                foreach (var action in _playerActions[type])
                {
                    action(feature);
                }
            }
        }

        return false;
    }

    public void AddPlayerAction<T>(Func<FeatureComponent, bool> action) where T : FeatureComponent => AddPlayerAction(typeof(T), action);
    public void AddPlayerAction(System.Type type,  Func<FeatureComponent, bool> action)
    {
        var key = type;
        if (!_playerActions.ContainsKey(key))
        {
            _playerActions[key] = new();
        }
        if (!_playerActions[key].Contains(action))
            _playerActions[key].Add(action);
    }
    public void AddConstantAction<T>(Func<FeatureComponent, bool> action) where T : FeatureComponent => AddConstantAction(typeof(T), action);
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
