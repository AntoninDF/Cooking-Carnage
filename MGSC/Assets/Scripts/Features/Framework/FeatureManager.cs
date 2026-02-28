using Features;
using System.Collections.Generic;
using System.Linq;
public class FeatureManager : IManager
{

    #region Fields

    protected Dictionary<System.Type, List<FeatureComponent>> trackedFeatures = new();



    #endregion


    #region Methods
    public void Init()
    {
        throw new System.NotImplementedException();
    }

    public void Register() => Reference.SetManager(this);

    public void Unregister() => Reference.RemoveManager(this);



    public bool TryGetAll<T>(out List<T> features) where T : FeatureComponent
    {
        features = GetAll<T>();
        if (features.Count == 0) return false;
        return true;
    }
    public List<FeatureComponent> GetAll()
    {
        List<FeatureComponent> result = new();
        foreach (var features in trackedFeatures.Values)
            result = result.Concat(features).ToList();
        return result;
    }
    public List<T> GetAll<T>() where T : FeatureComponent
    {
        if (!trackedFeatures.ContainsKey(typeof(T))) return new List<T>();
        List<T> result = new List<T>();
        foreach(FeatureComponent comp  in trackedFeatures[typeof(T)])
        {
            result.Add(comp as T);
        }
        return result;
    }
    public void Track<T>(T feature) where T : FeatureComponent
    {
        if (IsTracked(feature)) return;

        if (!trackedFeatures.ContainsKey(typeof(T)))
            trackedFeatures.Add(typeof(T), new List<FeatureComponent>());
        trackedFeatures[typeof(T)].Add(feature);
    }
    public void Untrack<T>(T feature) where T : FeatureComponent
    {
        if (!IsTracked(feature)) return;

        if (!trackedFeatures.ContainsKey(typeof(T))) return;
        if (trackedFeatures[typeof(T)] == null) return;
        trackedFeatures[typeof(T)].Remove(feature);
    }

    public bool IsTracked<T>(T feature) where T : FeatureComponent
    {
        if (trackedFeatures == null) return false;
        if (!trackedFeatures.ContainsKey(typeof(T))) return false;
        if (trackedFeatures[typeof(T)] == null) return false;
        if (!trackedFeatures[typeof(T)].Contains(feature)) return false;

        return true;
    }


    #endregion
}
