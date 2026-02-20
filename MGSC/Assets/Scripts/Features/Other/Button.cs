using UnityEngine.Events;

/// <summary>
/// Can be interacted with, triggers a unity event
/// </summary>
public class Button : Interactable
{
    public UnityEvent OnPress;

    public Button(IFeatureCollection owner) : base(owner)
    {
    }

    public override bool TryInteract()
    {
        if (base.TryInteract())
        {
            return true;
        }
        return false;
    }
}

