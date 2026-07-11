using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class SettingsMenu : UIMenuOverlay
	{


		[SerializeField, Required] DropdownOption DropdownPrefab;
		[SerializeField, Required] SliderOption SliderPrefab;

		[Space]
		[SerializeField] private Button B_Close;
		[SerializeField] private Transform G_Options;

		private List<SettingsGroup> _availableGroups = new();

        public override void OnLoad()
        {
            base.OnLoad();
            Bind();
			GenerateContent();
        }

        private void Bind()
        {
			if (B_Close != null)
				B_Close.onClick.AddListener(Disable);
        }

        void GenerateContent()
		{
			ClearContent();
			AddGroup("Audio")
				.AddSlider("Master volume", value => AudioSystem.SetGlobalParameter("MasterVolume", value))
				.AddSlider("SFX volume", value => AudioSystem.SetGlobalParameter("SFXVolume", value))
				.AddSlider("Ambiance volume", value => AudioSystem.SetGlobalParameter("AmbianceVolume", value))
				.AddSlider("Music volume", value => AudioSystem.SetGlobalParameter("MusicVolume", value))
				;
				
		}

		void ClearContent()
		{
			_availableGroups.Clear();
			
			for (int i = 0; i < G_Options.childCount; i++)
			{
				Destroy(G_Options.GetChild(i).gameObject);
			}
		}

		SettingsGroup AddGroup(string name)
		{
			var group = new SettingsGroup(name, G_Options, DropdownPrefab, SliderPrefab);
			_availableGroups.Add(group);
			return group;
		}

		private class SettingsGroup
		{
			public string Name = "New menu";
			public Transform ContentParent;
			public DropdownOption DropdownPrefab;
			public SliderOption SliderPrefab;
			public List<UIOption> Options = new();

			public SettingsGroup(string name, Transform contentParent, DropdownOption dropdownPrefab, SliderOption sliderPrefab)
			{
				Name = name;
				ContentParent = contentParent;
				DropdownPrefab = dropdownPrefab;
				SliderPrefab = sliderPrefab;
			}

			public T GetOption<T>(string name) where T : UIOption
			{
                foreach (var option in Options)
                {
                    if (option.Name == name && option is T result)
                        return result;
                }
                return null;
            }
			public UIOption GetOption(string name)
			{
				foreach (var option in Options)
				{
					if (option.Name == name)
						return option;
				}
				return null;
			}
			private T AddOption<T>(T prefab, string name) where T : UIOption
			{
                if (prefab == null)
                {
                    Debug.LogWarning("[SettingsMenu] Couldn't add a dropdown because the given prefab was null.");
                    return null;
                }
                var go = GameObject.Instantiate(prefab.gameObject);
				var rt = go.GetComponent<RectTransform>();
				rt.SetParent(ContentParent);
				rt.localScale = Vector3.one;
				
                T option = go.GetComponent<T>();
				option.Name = name;

				return option;
            }

			public SettingsGroup AddDropdown(string name, params (string name, Action onOptionSelected)[] options)
			{
				var prefab = DropdownPrefab;
				var option = AddOption(prefab, name);
				if (option == null) return this;

				option.SetOptions(options);

				return this;
			}
			public SettingsGroup AddDropdown<EnumType>(string name, params Action[] callbacks) where EnumType : Enum
            {
                var prefab = DropdownPrefab;
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

			public SettingsGroup AddSlider(string name, Action<float> onValueChange, float defaultValue = 0f)
            {
                var prefab = SliderPrefab;
                var option = AddOption(prefab, name);
                if (option == null) return this;

				option.Setup(name, onValueChange);
				option.SetValue(defaultValue);
				return this;
            }
		}
    }

}