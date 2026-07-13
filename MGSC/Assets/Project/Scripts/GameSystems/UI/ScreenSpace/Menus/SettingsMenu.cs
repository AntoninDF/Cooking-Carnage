using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
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
				.AddSlider("Master volume", AudioSystem.SetMasterVolume, 1f, SliderOption.DisplayMode.Percentage)
				.AddSlider("SFX volume", AudioSystem.SetSFXVolume, 1f, SliderOption.DisplayMode.Percentage)
				.AddSlider("Ambience volume", AudioSystem.SetAmbienceVolume, 1f, SliderOption.DisplayMode.Percentage)
				.AddSlider("Music volume", AudioSystem.SetMusicVolume, 1f, SliderOption.DisplayMode.Percentage)
				.AddSlider("Voice volume", AudioSystem.SetVoiceVolume, 1f, SliderOption.DisplayMode.Percentage)
				;

			var resolutionOptions = new (string, Action)[Screen.resolutions.Length];
			int i = 0;
			foreach (var rez in Screen.resolutions)
			{
				(string, Action) newValue = (rez.width + " x " + rez.height, () => Screen.SetResolution(rez.width, rez.height, Screen.fullScreen));

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
				.AddSlider("Camera sensitivity", Globals.Gameplay.Player.LookSpeed.Set, 0.1f, SliderOption.DisplayMode.Percentage)
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

		
    }

}