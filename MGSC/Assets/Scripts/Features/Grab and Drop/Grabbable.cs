using System;

public class Grabbable : FeatureComponent
{
    public Container Container;
    public bool IsLocked { get; set; }

    public Action OnGrab {  get; set; }

    public Grabbable(IFeatureCollection owner) : base(owner)
    {
    }

    public void GetGrabbedBy(Container container)
    {
        if (Container == container) return;
        if (IsLocked) return;

        Container?.RequestDrop(this);
        Container = container;

        OnGrab?.Invoke();
    }
}
