using UI;
using UnityEngine;

//This script caused error due to having the same namespace
namespace UI2
{
    [HideScriptField]
    public class HeadsUpDisplay : UIHUD
    {
        public void OnLoad()
        {
            Enable();
        }
    } 
}
