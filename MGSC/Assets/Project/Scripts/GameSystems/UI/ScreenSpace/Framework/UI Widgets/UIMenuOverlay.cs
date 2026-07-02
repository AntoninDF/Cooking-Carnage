using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(GraphicRaycaster))]
    public class UIMenuOverlay : UIWidget
	{
        public UIMenuOverlay()
        {
            OnEnable += () => CursorUtility.RequestCursor(this);
            OnDisable += () => CursorUtility.TryFreeCursor(this);
        }
    }

}