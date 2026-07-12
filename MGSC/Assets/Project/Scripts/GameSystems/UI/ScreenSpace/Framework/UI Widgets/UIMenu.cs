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
        protected override void OnReset()
        {
            GetComponent<Canvas>().sortingOrder = 20;
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