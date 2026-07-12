using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(GraphicRaycaster))]
    public class UIMenuOverlay : FullscreenWidget
	{
        public UIMenuOverlay()
        {
            OnEnable += () => CursorUtility.RequestCursor(this);
            OnDisable += () => CursorUtility.TryFreeCursor(this);
        }
        protected override void OnReset()
        {
            GetComponent<Canvas>().sortingOrder = 30;
        }
    }

}