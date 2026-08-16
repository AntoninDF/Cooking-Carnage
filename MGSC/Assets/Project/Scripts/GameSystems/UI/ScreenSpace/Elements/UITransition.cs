using GameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
	public class UITransition : UIMenuOverlay
	{
		public GameObject G_Content;
		public TMP_Text L_Title;
		public TMP_Text L_Description;

		[SerializeField] UnityEvent OnShow;
        [SerializeField] UnityEvent OnHide; 

		#region Methods

		public void Show(string title,  string description="")
		{
			Enable();
			if (L_Title != null)
				L_Title.text = title;
			if (L_Description != null)
			{
				if (!string.IsNullOrEmpty(description))
				{
					L_Description.gameObject.SetActive(true);
					L_Description.text = description;
				}
				else
					L_Description.gameObject.SetActive(false);
			}

			OnShow?.Invoke();
		}

		public void Hide()
        {
            OnHide?.Invoke();
			TimerManager.StartTimer("Hide UI transition", 1f, Disable);
		}

		#endregion
	}

}