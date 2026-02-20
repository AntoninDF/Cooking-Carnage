using System.Collections.Generic;
using System.Linq;

public class FeatureCollection
{
    #region Properties

    public List<FeatureComponent> DirectFeatures { get; set; } = new();
    public List<FeatureComponent> AllFeatures
    {
        get
        {
            List<FeatureComponent> result = new();
            foreach (var feature in DirectFeatures)
            {
                if (!result.Contains(feature)) result.Add(feature);
                if (feature is IFeatureCollection col)
                    foreach(var colFeature in col.Collection.AllFeatures)
                        if (!result.Contains(colFeature))
                            result.Add(colFeature);
            }
            return result;
        }
    }


    #endregion

    #region Methods

    public void Init()
    {
        foreach (var feature in AllFeatures)
            feature.Init();
    }
    public void Update()
    {
        foreach (var feature in AllFeatures)
            if (feature.Enabled)
                feature.Update();
    }

    public void Add(FeatureComponent feature)
    { 
        if (!DirectFeatures.Contains(feature)) 
            DirectFeatures.Add(feature); 
    }
    public void Remove(FeatureComponent feature) => DirectFeatures.Remove(feature);
    public T Get<T>() where T : FeatureComponent
    {
        foreach(var feature in DirectFeatures)
            if (feature is T)
                return (T)feature;

        return null;
    }
    public bool TryGet<T>(out T feature) where T : FeatureComponent
    {
        feature = Get<T>();
        return feature != null;
    }
    #endregion

}
