using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
		[SerializeField] private TMP_Text L_CurrentMenu;

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
				.AddSlider("Master volume",		AudioSystem.SetMasterVolume,	Globals.Audio.MasterVolume,		SliderOption.DisplayMode.Percentage)
				.AddSlider("SFX volume",		AudioSystem.SetSFXVolume,		Globals.Audio.SFXVolume,		SliderOption.DisplayMode.Percentage)
				.AddSlider("Ambience volume",	AudioSystem.SetAmbienceVolume,	Globals.Audio.AmbienceVolume,	SliderOption.DisplayMode.Percentage)
				.AddSlider("Music volume",		AudioSystem.SetMusicVolume,		Globals.Audio.MusicVolume,		SliderOption.DisplayMode.Percentage)
				.AddSlider("Voice volume",		AudioSystem.SetVoiceVolume,		Globals.Audio.DialogVolume,		SliderOption.DisplayMode.Percentage)
				;

			// Only 16:9 resolution options
			var resolutions = new (string name, int width, int height, FullScreenMode screenMode)[]
			{
				("4K", 3840, 2160, FullScreenMode.ExclusiveFullScreen),
				("QHD", 2560, 1440, FullScreenMode.ExclusiveFullScreen),
				("Full HD", 1920, 1080, FullScreenMode.ExclusiveFullScreen),
				("", 1600, 900, FullScreenMode.ExclusiveFullScreen),
				("", 1536, 864, FullScreenMode.ExclusiveFullScreen),
				("", 1366, 768, FullScreenMode.ExclusiveFullScreen),
				("HD", 1280, 720, FullScreenMode.ExclusiveFullScreen),
				("Full HD (windowed)", 1920, 1080, FullScreenMode.MaximizedWindow),
				("Windowed", 1067, 600, FullScreenMode.Windowed),
				("Windowed", 800, 450, FullScreenMode.Windowed)
			};

            var resolutionOptions = new (string, Action)[resolutions.Length];
			int i = 0;
			foreach (var rez in resolutions)
			{
				string name = rez.width + " x " + rez.height;
				if (!string.IsNullOrEmpty(rez.name))
					name = rez.name + " - " + name;
                (string, Action) newValue = (name, () => Screen.SetResolution(rez.width, rez.height, rez.screenMode));

				if (resolutionOptions.Contains(newValue))
					continue;
				resolutionOptions[i] = newValue;
				i++;
			}

            AddGroup("Graphics")
				.AddDropdown<FullScreenMode>("Window mode", value => Screen.fullScreenMode = (FullScreenMode)value, FullScreenMode.ExclusiveFullScreen)
				.AddDropdown("Resolution", resolutionOptions, 999) // Set value to the maximum
				;

			AddGroup("Controls")
				.AddSlider("Camera sensitivity", Globals.Gameplay.Player.LookSpeed.Set,			Globals.Gameplay.Player.LookSpeed,			SliderOption.DisplayMode.Integer, new Vector2(1, 100), new Vector2(0.01f, 0.2f))
				.AddSlider("Scroll sensitivity", Globals.Gameplay.Player.ScrollSensitivity.Set, Globals.Gameplay.Player.ScrollSensitivity,	SliderOption.DisplayMode.Integer, new Vector2(1, 100))
				;

			if (_availableGroups.Count > 0 && _currentActiveGroup == null)
				EnableGroup(_availableGroups[0]);

			GetGroup("Graphics")?.GetOption<DropdownOption>("Window mode")?.SetValue((int)Screen.fullScreenMode);
			//Debug.Log("Fullscreen mode is " + Enum.GetName(typeof(FullScreenMode), Screen.fullScreenMode) + ", value is " + (int)Screen.fullScreenMode);
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
			if (L_CurrentMenu != null)
				L_CurrentMenu.text = group.Name;

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

		SettingsGroup GetGroup(string name)
		{
			foreach (var group in _availableGroups)
				if (group.Name == name)
					return group;
			return null;
		}
    }

}