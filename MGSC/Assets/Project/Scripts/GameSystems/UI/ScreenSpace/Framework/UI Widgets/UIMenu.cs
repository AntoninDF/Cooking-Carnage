using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(GraphicRaycaster))]
    public class UIMenu : FullscreenWidget
    {
		public UIMenu()
		{
			OnEnable += OnMenuOpen;
			OnDisable += OnMenuClose;
		}
		protected virtual void OnMenuOpen()
		{
            CursorUtility.RequestCursor(this);
			GameLoop.RequestPause(this);
        }
		protected virtual void OnMenuClose()
		{
            CursorUtility.TryFreeCursor(this);
			GameLoop.TryResume(this);
        }
	}

}