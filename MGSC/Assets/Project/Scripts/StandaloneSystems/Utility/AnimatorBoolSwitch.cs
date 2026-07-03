using UnityEngine;

public class AnimatorBoolSwitch : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private string _paramName = "IsActive";

    public void SetTrue() => _animator.SetBool(_paramName, true);
    public void SetFalse() => _animator.SetBool(_paramName, false);
}
