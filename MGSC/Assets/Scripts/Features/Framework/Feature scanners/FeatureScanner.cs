using System.Collections.Generic;

namespace Features
{
    public class FeatureScanner
    {
        #region Methods

        public virtual List<Feature> Get(out List<Feature> list)
        {
            list = Get();
            return list;
        }
        public virtual List<Feature> Get()
        {
            var manager = Reference.GetManager<FeatureManager>();
            return manager.GetAll();
        }

        public virtual List<T> Get<T>(out List<T> list) where T : Feature
        {
            list = Get<T>();
            return list;
        }
        public virtual List<T> Get<T>() where T : Feature
        {
            List<T> result = new();

            foreach (var component in Get())
            {
                if (component is T)
                    result.Add((T)component);
            }
            return result;
        }


        #endregion
    }
}