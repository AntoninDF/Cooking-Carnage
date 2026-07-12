using System;
using TMPro;
using UnityEngine;

namespace UI
{
	public class UIOption : UIWidget
	{
		public string Name
		{
			get => optionName;
			set
			{
				optionName = value;
				if (L_Label != null)
					L_Label.text = value;
			}
		}
		protected string optionName = "Unnamed option";

		public TMP_Text L_Label;
	}
}