using NaughtyAttributes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;

namespace UI
{
	public class SettingsMenu : FullscreenWidget
	{


		[SerializeField, Required] DropdownOption DropdownPrefab;
		[SerializeField, Required] SliderOption SliderPrefab;

		[Space]
		[SerializeField] private Transform G_Options;

		private List<SettingsGroup> _availableGroups = new();

        public override void OnLoad()
        {
            base.OnLoad();
        }

		void GenerateContent()
		{

		}

		SettingsGroup AddGroup(string name)
		{
			var group = new SettingsGroup(name);
			_availableGroups.Add(group);
			return group;
		}

		private class SettingsGroup
		{
			public string Name = "New menu";
			public List<UIOption> Options = new();

			public SettingsGroup(string name)
			{
				Name = name;
			}

			private T AddOption<T>(T prefab, string name) where T : UIOption
			{
                if (prefab == null)
                {
                    Debug.LogWarning("[SettingsMenu] Couldn't add a dropdown because the given prefab was null.");
                    return null;
                }
                var go = GameObject.Instantiate(prefab.gameObject);
                T option = go.GetComponent<T>();
				option.Name = name;

				return option;
            }

			public SettingsGroup AddDropdown(DropdownOption prefab, string name, params (string name, Action onOptionSelected)[] options)
			{
				var option = AddOption(prefab, name);
				if (option == null) return this;

				option.SetOptions(options);

				return this;
			}
			public SettingsGroup AddDropdown<EnumType>(DropdownOption prefab, string name, params Action[] callbacks) where EnumType : Enum
			{
				var option = AddOption(prefab, name);
				if (option == null) return this;

				string[] names = Enum.GetNames(typeof(EnumType));

                for (int i = 0; i < names.Length; i++)
				{
					if (i < callbacks.Length)
						option.AddOption(names[i], callbacks[i]);
					else
						option.AddOption(names[i]);
				}

				return this;
			}
		}
    }

}