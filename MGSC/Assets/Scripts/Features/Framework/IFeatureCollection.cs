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
        public T Get<T>() where T : Feature => Collection.Get<T>();
        public T Get<T>(out T feature) where T : Feature { feature = Collection.Get<T>(); return feature; }
        public bool TryGet<T>(out T feature) where T : Feature => Collection.TryGet(out feature);
        public void Init() => Collection.Init();
        public void Update() => Collection.Update();
    }
}