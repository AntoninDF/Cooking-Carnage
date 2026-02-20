using System;

public class Interactable : FeatureComponent
{

    public Action OnInteraction {  get; set; }


    public Interactable(IFeatureCollection owner) : base(owner)
    {
    }

    public virtual bool TryInteract()
    {
        if (!CanInteract()) return false;

        OnInteraction?.Invoke();

        return true;
    }

    public virtual bool CanInteract()
    {
        return Enabled;
    }

}
