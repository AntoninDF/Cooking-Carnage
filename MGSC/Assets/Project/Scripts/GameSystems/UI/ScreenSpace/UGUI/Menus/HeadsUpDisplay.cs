using UnityEngine;

namespace UI
{
    [HideScriptField]
    public class HeadsUpDisplay : UIHUD
    {
        override void OnLoad()
        {
            Enable();
        }
    } 
}
