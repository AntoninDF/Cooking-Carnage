using UnityEngine;

namespace UI
{
	public class UIHUDOverlay : FullscreenWidget
    {
        protected override void OnReset()
        {
            GetComponent<Canvas>().sortingOrder = 10;
        }
    }

}