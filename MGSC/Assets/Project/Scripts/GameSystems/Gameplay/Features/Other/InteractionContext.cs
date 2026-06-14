namespace Features
{
    public class InteractionContext
    {
        public object Interactor { get; set; }


        public InteractionContext(object interactor)
        {
            Interactor = interactor;
        }

        public T As<T>(out T obj) where T : class
        {
            obj = Interactor as T;
            return obj;
        }
        public bool Is<T>() where T : class
        {
            return Interactor is T;
        }
        public bool TryGetFeature<T>(out T obj) where T : Feature
        {
            obj = null;
            if (Is<T>())
            {
                return As(out obj) != null;
            }
            if (Is<IFeatureCollection>())
            {
                return As<IFeatureCollection>(out var temp).TryGet(out obj);
            }
            return false;
        }
    }
}