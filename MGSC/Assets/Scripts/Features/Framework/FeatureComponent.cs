

using System;

public class FeatureComponent
{
    #region Properties


    public FeatureManager Manager
    {
        get
        {
            if (manager == null)
                Reference.GetManager(out manager);
            return manager;
        }

        set => manager = value;
    }

    public IFeatureCollection Owner { get; set; }

    public bool Enabled {
        get => enabled;
        set
        {
            if (enabled != value)
            {
                enabled = value;
                if (enabled) OnEnable?.Invoke();
                else OnDisable?.Invoke();
            }
            
        }
    }
    public Action OnEnable { get; set; }
    public Action OnDisable { get; set; }
    #endregion


    #region Fields

    protected FeatureManager manager;
    protected bool enabled = true;

    #endregion

    #region Methods

    public FeatureComponent(IFeatureCollection owner)
    {
        Init();
        Owner = owner;
        Owner.Collection.Add(this);
    }

    public virtual void Init() { }
    public virtual void Update() { }
    public virtual void Track() => Manager.Track(this);
    public virtual void Untrack() => Manager.Untrack(this);
    public virtual void IsTracked() => Manager.IsTracked(this);
    public T GetSibling<T>() where T : FeatureComponent
    {
        if (Owner == null) return null;
        return Owner.Get<T>();
    }
    public bool TryGetSibling<T>(out T feature) where T : FeatureComponent
    {
        feature = null;
        if (Owner == null) return false;
        return Owner.TryGet(out feature);
    }

    #endregion
}
