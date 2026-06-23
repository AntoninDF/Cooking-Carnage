using System;
using UnityEngine;
public class TestEventBroadcaster : MonoBehaviour
{
    [GlobalEvent("TEST/Action1")]
    public static Action MyAction;
    [GlobalEvent("Action2")]
    public static Action MyOtherAction;
    [GlobalEvent("Action3")]
    public static Action MyNewAction;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) )
        {
            Debug.Log("Space pressed");
            MyAction?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.LeftControl) )
        {
            Debug.Log("LeftControl pressed");
            MyOtherAction?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.RightControl) )
        {
            Debug.Log("RightControl pressed");
            MyNewAction?.Invoke();
        }
    }
    

}