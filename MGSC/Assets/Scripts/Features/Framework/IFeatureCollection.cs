public interface IFeatureCollection
{
    public FeatureCollection Collection { get; }
    public void Add(FeatureComponent feature) => Collection.Add(feature);
    public void Remove(FeatureComponent feature) => Collection.Remove(feature);
    public T Get<T>() where T : FeatureComponent => Collection.Get<T>();
    public bool TryGet<T>(out T feature) where T : FeatureComponent => Collection.TryGet(out feature);
    public void Init() => Collection.Init();
    public void Update() => Collection.Update();
}
