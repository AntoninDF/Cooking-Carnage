using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class SettingsMenu : UIMenuOverlay
	{


		[SerializeField, Required] ConfigurableButton MenuNamePrefab;
		[SerializeField, Required] DropdownOption DropdownPrefab;
		[SerializeField, Required] SliderOption SliderPrefab;

		[Space]
		[SerializeField] private Button B_Close;
		[SerializeField] private Transform G_Options;
		[SerializeField] private Transform G_MenuNames;

		private List<SettingsGroup> _availableGroups = new();
		private SettingsGroup _currentActiveGroup;

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
				.AddSlider("Master volume", AudioSystem.SetMasterVolume)
				.AddSlider("SFX volume", AudioSystem.SetSFXVolume)
				.AddSlider("Ambience volume", AudioSystem.SetAmbienceVolume)
				.AddSlider("Music volume", AudioSystem.SetMusicVolume)
				.AddSlider("Voice volume", AudioSystem.SetVoiceVolume)
				;

			var resolutionOptions = new (string, Action)[Screen.resolutions.Length];
			int i = 0;
			foreach (var rez in Screen.resolutions)
			{
				resolutionOptions[i] = (rez.width + " x " + rez.height, () => Screen.SetResolution(rez.width, rez.height, Screen.fullScreen));
				i++;
			}

            AddGroup("Graphics")
				.AddDropdown<FullScreenMode>("Window mode", value => Screen.fullScreenMode = (FullScreenMode)value)
				.AddDropdown("Resolution", resolutionOptions)
				;

			if (_availableGroups.Count > 0 && _currentActiveGroup == null)
				EnableGroup(_availableGroups[0]);

		}

		void ClearContent()
		{
			_availableGroups.Clear();
			
			for (int i = 0; i < G_Options.childCount; i++)
			{
				Destroy(G_Options.GetChild(i).gameObject);
			}
			
			for (int i = 0; i < G_MenuNames.childCount; i++)
			{
				Destroy(G_MenuNames.GetChild(i).gameObject);
			}
		}
		void EnableGroup(SettingsGroup group)
		{
			foreach (var curr in _availableGroups)
			{
				if (curr == group)
					curr.Enable();
				else
					curr.Disable();
			}
			_currentActiveGroup = group;
		}
		void EnableGroup(string name)
		{
			foreach (var group in _availableGroups)
				if (group.Name == name)
					EnableGroup(group);
		}
		SettingsGroup AddGroup(string name)
		{
			var title = Instantiate(MenuNamePrefab, G_MenuNames);

			var group = new SettingsGroup(name, title, G_Options, DropdownPrefab, SliderPrefab);
            group.GroupButton.Setup(name, () => EnableGroup(group));

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
			public ConfigurableButton GroupButton;
			public RectTransform Container;
			public bool enabled;

			public SettingsGroup(string name, ConfigurableButton groupButton, Transform contentParent, DropdownOption dropdownPrefab, SliderOption sliderPrefab)
			{
				Name = name;
				GroupButton = groupButton;
				ContentParent = contentParent;
				DropdownPrefab = dropdownPrefab;
				SliderPrefab = sliderPrefab;

				SetupContainer(name);
			}

			public void Enable()
			{
				enabled = true;
				Container.gameObject.SetActive(true);
			}
			public void Disable()
			{
				enabled = false;
				Container.gameObject.SetActive(false);
			}
			void SetupContainer(string name)
			{
				if (Container != null)
					Destroy(Container);

				Container = new GameObject(
					name + " (Container content)",
					typeof(RectTransform))
                    .GetComponent<RectTransform>();
				Container.SetParent(ContentParent);
				Container.localScale = Vector2.one;
				Container.anchorMin = Vector2.zero;
				Container.anchorMax = Vector2.one;
				Container.sizeDelta = Vector2.zero;
				Container.anchoredPosition = Vector2.zero;

				var layout = Container.gameObject.AddComponent<VerticalLayoutGroup>();
				layout.spacing = 20;
				int padding = 50;
				layout.padding = new RectOffset(padding, padding, padding, padding);
				layout.childControlHeight = false;
				layout.childControlWidth = true;
				layout.childForceExpandHeight = false;
				layout.childForceExpandWidth = true;
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
                var go = GameObject.Instantiate(prefab.gameObject, Container);
				var rt = go.GetComponent<RectTransform>();
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
			public SettingsGroup AddDropdown<EnumType>(string name, Action<int> onChange) where EnumType : Enum
            {
                var prefab = DropdownPrefab;
                var option = AddOption(prefab, name);
				if (option == null) return this;

				string[] names = Enum.GetNames(typeof(EnumType));

                for (int i = 0; i < names.Length; i++)
				{
					option.AddOption(names[i]);
				}
				option.OnValueChanged += onChange;

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