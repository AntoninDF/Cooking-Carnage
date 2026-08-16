using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class DropdownOption : UIOption
	{




		[SerializeField] private TMP_Dropdown Dropdown;
		public Action<int> OnValueChanged = value => { };
		public List<string> OptionNames => Dropdown.options.Select(obj => obj.text).ToList();
		public int OptionCount => Dropdown.options.Count;
		private Dictionary<int, Action> _optionCallbacks = new();
        private void Start()
        {
			if (Dropdown == null) return;
			Dropdown.onValueChanged.AddListener(OnValueChanged.Invoke);
			Dropdown.onValueChanged.AddListener(Apply);
        }
		public void Setup(string name, Action<int> onValueChange)
		{
			Name = name;
			OnValueChanged += onValueChange;
		}
        public int GetValue() => Dropdown != null ? Dropdown.value : -1;
		public void SetValue(int value)
		{
			if (Dropdown == null) return;
			// Callbacks aren't called if we assign the same value, so we make sure they are
			// it's useful when the content of the dropdown changes and we want to apply the change without really changing the value
			if (Dropdown.value == value)
				Apply(value);
			else
				Dropdown.value = value;
		}
		public void ClearOptions()
		{
			if (Dropdown == null) return;
			Dropdown.ClearOptions();
			_optionCallbacks.Clear();
        }
		public void AddOption(string name, Action action = null)
		{
			if (Dropdown == null) return;
			_optionCallbacks[Dropdown.options.Count] = action;
			Dropdown.options.Add(new TMP_Dropdown.OptionData(name));
			Dropdown.RefreshShownValue();
		}
		public void SetOptions(params string[] options)
		{
			foreach (var option in options)
				AddOption(option);
		}
		public void SetOptions<EnumType>() where EnumType : Enum
		{
			foreach (var option in Enum.GetNames(typeof(EnumType)))
				AddOption(option);
		}
		public void SetOptions(params (string name, Action onOptionSelected)[] options)
		{
			ClearOptions();
			for (int i = 0; i < options.Length; i++)
			{
				var option = options[i];
				AddOption(option.name, option.onOptionSelected);
			}
		}
		void Apply(int value)
		{
			if (_optionCallbacks.ContainsKey(value))
				_optionCallbacks[value]?.Invoke();
		}
    }

}