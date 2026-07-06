using GameSystem;
using UnityEngine;

[RequireComponent(typeof(InteractionButton), typeof(SphereCollider), typeof(MeshRenderer))]
public class Todelete : MonoBehaviour
{
    InteractionButton button;
    private void Start()
    {
        if (TryGetComponent(out button))
        {
            button.OnInteraction += Reset;
        }
    }

    private void Reset(object obj)
    {
        Debug.Log("Try to reset");
        if (Reference.TryGetManager(out ShiftManager mgr))
        {
            mgr.ResetAll();
        }
    }
}
