

using System;
using UnityEngine;
public class TestEventBroadcaster : MonoBehaviour
{
    [BindEvent("TEST/Action1")]
    public Action MyAction;
    [BindEvent("Action2")]
    public Action MyOtherAction;
    [BindEvent("Action3")]
    public Action MyNewAction;

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