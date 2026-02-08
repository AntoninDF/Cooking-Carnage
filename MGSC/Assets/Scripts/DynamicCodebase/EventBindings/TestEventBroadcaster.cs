

using System;
using UnityEngine.Events;

public class TestEventBroadcaster
{
    [BindEvent("MyAction")]
    public Action MyAction;
    [BindEvent("FirstFolder/MyOtherAction")]
    public Action MyOtherAction;

}