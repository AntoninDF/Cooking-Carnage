using System.Collections.Generic;

namespace Features
{
    public class FeatureCollection
    {
        #region Properties

        public List<Feature> DirectFeatures { get; set; } = new();
        public List<Feature> AllFeatures
        {
            get
            {
                List<Feature> result = new();
                foreach (var feature in DirectFeatures)
                {
                    if (!result.Contains(feature)) result.Add(feature);
                    if (feature is IFeatureCollection col)
                        foreach (var colFeature in col.Collection.AllFeatures)
                            if (!result.Contains(colFeature))
                                result.Add(colFeature);
                }
                return result;
            }
        }


        #endregion

        #region Methods
        /// <summary>
        /// LateSetup should be called in Behaviour.Start()
        /// </summary>
        public void LateSetup()
        {
            foreach (var feature in AllFeatures)
                feature.LateSetup();
        }
        /// <summary>
        /// Update should be called in Behaviour.Update()
        /// </summary>
        public void Update()
        {
            foreach (var feature in AllFeatures)
                if (feature.Enabled)
                    feature.Update();
        }

        public void Add(Feature feature)
        {
            if (!DirectFeatures.Contains(feature))
                DirectFeatures.Add(feature);
        }
        public void Remove(Feature feature) => DirectFeatures.Remove(feature);
        public T Get<T>(bool mustBeEnabled = false, bool scanOnlyDirectFeatures = false) where T : Feature
        {
            foreach (var feature in scanOnlyDirectFeatures?DirectFeatures:AllFeatures)
                if (feature is T)
                    if ((mustBeEnabled && feature.Enabled) || !mustBeEnabled)
                        return (T)feature;

            return null;
        }
        public bool TryGet<T>(out T feature, bool mustBeEnabled = false, bool scanOnlyDirectFeatures = false) where T : Feature
        {
            feature = Get<T>(mustBeEnabled, scanOnlyDirectFeatures);
            return feature != null;
        }
        #endregion

    }
}