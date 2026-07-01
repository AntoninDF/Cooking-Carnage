using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(GraphicRaycaster))]
    public class UIMenu : UIWidget
	{
		public UIMenu()
		{
			OnEnable += () => CursorUtility.RequestCursor(this);
			OnDisable += () => CursorUtility.TryFreeCursor(this);
		}
	}

}