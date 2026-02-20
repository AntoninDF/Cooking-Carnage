using System.Collections.Generic;

public class FeatureScanner
{
    #region Methods

    public virtual List<FeatureComponent> Get()
    {
        var manager = Reference.GetManager<FeatureManager>();
        return manager.GetAll();
    }
    public virtual List<T> Get<T>() where T : FeatureComponent
    {
        List<T> result = new();

        foreach(var component in Get())
        {
            if (component is T)
                result.Add((T)component);
        }
        return result;
    }


    #endregion
}
