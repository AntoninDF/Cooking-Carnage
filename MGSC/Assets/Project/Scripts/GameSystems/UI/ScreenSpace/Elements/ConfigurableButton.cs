using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class ConfigurableButton : UIWidget
	{


		[SerializeField] private Button B_Button;
		[SerializeField] private TMP_Text L_Text;

		public void Setup(string name, Action onClick)
		{
			if (L_Text != null)
				L_Text.text = name;

			if (B_Button != null)
			{
				B_Button.onClick.AddListener(() => onClick());
				B_Button.onClick.AddListener(() => Debug.Log("Configurable button setup"));
			}
		}

	}

}