using UnityEngine;

public interface IGrabbable
{
    public Transform Transform { get; }
    public void Throw(Interactor interactor);
    public void TryThrow(Interactor interactor);
}
