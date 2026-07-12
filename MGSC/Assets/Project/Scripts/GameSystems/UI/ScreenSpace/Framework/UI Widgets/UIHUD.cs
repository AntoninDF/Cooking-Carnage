using UnityEngine;

namespace UI
{
	public class UIHUD : FullscreenWidget
    {
        protected override void OnReset()
        {
            GetComponent<Canvas>().sortingOrder = 0;
        }
    }

}