using System.Collections.Generic;

namespace Features
{
    public interface IFeatureCollection
    {
        public FeatureCollection Collection { get; }
        public List<Feature> DirectFeatures => Collection.DirectFeatures;
        public List<Feature> AllFeatures => Collection.AllFeatures;
        public void Add(Feature feature) => Collection.Add(feature);
        public void Remove(Feature feature) => Collection.Remove(feature);
        /// <summary>
        /// Retrieves the first corresponding feature from the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(bool mustBeEnabled=false) where T : Feature => Collection.Get<T>(mustBeEnabled);
        public T Get<T>(out T feature, bool mustBeEnabled = false) where T : Feature { feature = Collection.Get<T>(mustBeEnabled); return feature; }
        public bool TryGet<T>(out T feature, bool mustBeEnabled = false) where T : Feature => Collection.TryGet(out feature, mustBeEnabled);
        public void LateSetup() => Collection.LateSetup();
        public void Update() => Collection.Update();
    }
}