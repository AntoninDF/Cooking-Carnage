using GameSystem;
using UnityEngine;

public class ShiftRequester : MonoBehaviour
{

    [SerializeField] private InteractionButton _button;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_button != null)
        {
            _button.OnInteraction += _ => RequestShiftLaunch();
        }
    }

    void RequestShiftLaunch()
    {
        //        Debug.Log("TryRequestShiftLaunch");
        GameLoop.RequestTransition(StateTransitions.StartShiftCountdown);
    }
}
