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
		[SerializeField, Required] Scrollbar VerticalScrollbarPrefab;

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

            AddAudioGroup();
            AddGraphicsGroup();
            AddControlsGroup();
            AddDifficultyGroup();

            // Enable a default group
            if (_availableGroups.Count > 0 && _currentActiveGroup == null)
                EnableGroup(_availableGroups[0]);
        }

        private void AddDifficultyGroup()
        {
            // Difficulty
            AddGroup("Difficulty")
                .AddDropdown("Apply preset", GetDifficultyOptions(), 1)
                .AddSlider("Upgrade price", Globals.BalancingWeights.UpgradePriceMultiplier.Set, 1f, SliderOption.DisplayMode.Percentage, new Vector2(0.35f, 4f), new Vector2(0.35f, 4f), 2)
                .AddSlider("Recipe price", Globals.BalancingWeights.RecipePriceMultiplier.Set, 1f, SliderOption.DisplayMode.Percentage, new Vector2(0.35f, 4f), new Vector2(0.35f, 4f), 2)
                .AddSlider("Shift duration", Globals.BalancingWeights.ShiftDurationMultiplier.Set, 1f, SliderOption.DisplayMode.Minutes, new Vector2(3, 8), new Vector2(3f / 5f, 8f / 5f), 1)
                .AddSlider("Rent price", Globals.BalancingWeights.RentPriceMultiplier.Set, 1f, SliderOption.DisplayMode.Percentage, new Vector2(0.35f, 4f), new Vector2(0.35f, 4f), 2)
                .AddSlider("Client patience", Globals.BalancingWeights.ClientPatienceMultiplier.Set, 1f, SliderOption.DisplayMode.Percentage, new Vector2(0.5f, 2f), new Vector2(0.5f, 2f), 2)
                .AddSlider("Preparation fail time", Globals.BalancingWeights.PreparationFailTimeMultiplier.Set, 1f, SliderOption.DisplayMode.Percentage, new Vector2(3f, 0.25f), new Vector2(3f, 0.25f), 2)
                ;
        }

        private void AddControlsGroup()
        {
            // Controls
            AddGroup("Controls")
                .AddSlider("Camera sensitivity", Globals.Gameplay.Player.LookSpeed.Set, Globals.Gameplay.Player.LookSpeed, SliderOption.DisplayMode.Integer, new Vector2(1, 100), new Vector2(0f, 0.2f), 2)
                .AddSlider("Scroll sensitivity", Globals.Gameplay.Player.ScrollSensitivity.Set, Globals.Gameplay.Player.ScrollSensitivity, SliderOption.DisplayMode.Integer, new Vector2(1, 100), new Vector2(1f, 100f), 0)
                ;
        }

        private void AddGraphicsGroup()
        {
            // Graphics
            var resolutions = GetAvailableResolutions();
            (string, Action)[] resolutionOptions = GetResolutionDropdownItems(resolutions);
            AddGroup("Graphics")
                .AddDropdown<FullScreenMode>("Window mode", value => Screen.fullScreenMode = (FullScreenMode)value, FullScreenMode.ExclusiveFullScreen)
                .AddDropdown("Resolution", resolutionOptions, 999) // Set value to the maximum
                .AddDropdown("Target FPS", GetTargetFPSOptions(), 0)
                .AddDropdown("Vertical sync (VSync)", GetVSyncOptions(GetGroup("Graphics").GetOption<DropdownOption>("Target FPS")), 0)
                .AddDropdown("Graphics quality", GetGraphicsQualityOptions(), 0);
                ;

            // Graphics (default settings)
            GetGroup("Graphics")?.GetOption<DropdownOption>("Window mode")?.SetValue((int)Screen.fullScreenMode);
            GetGroup("Graphics")?.GetOption<DropdownOption>("Resolution")?.SetValue(2); // 2 is full HD
        }

        private void AddAudioGroup()
        {
            // Audio
            AddGroup("Audio")
                .AddSlider("Master volume", AudioSystem.SetMasterVolume, Globals.Audio.MasterVolume, SliderOption.DisplayMode.Percentage)
                .AddSlider("SFX volume", AudioSystem.SetSFXVolume, Globals.Audio.SFXVolume, SliderOption.DisplayMode.Percentage)
                .AddSlider("Ambience volume", AudioSystem.SetAmbienceVolume, Globals.Audio.AmbienceVolume, SliderOption.DisplayMode.Percentage)
                .AddSlider("Music volume", AudioSystem.SetMusicVolume, Globals.Audio.MusicVolume, SliderOption.DisplayMode.Percentage)
                .AddSlider("Voice volume", AudioSystem.SetVoiceVolume, Globals.Audio.DialogVolume, SliderOption.DisplayMode.Percentage)
                ;
        }
        #region Other


        private (string name, Action onOptionSelected)[] GetGraphicsQualityOptions()
        {
            var result = new (string name, Action onOptionSelected)[]
                {
                    ("High", () => QualitySettings.SetQualityLevel(0)),
                    ("Medium", () => QualitySettings.SetQualityLevel(1)),
                    ("Low", () => QualitySettings.SetQualityLevel(2))
                };
            return result;
        }
        private (string name, Action onOptionSelected)[] GetTargetFPSOptions()
        {
            var result = new (string name, Action onOptionSelected)[]
                {
                    ("Unlimited fps", () => Application.targetFrameRate = -1),
                    ("60 fps", () => Application.targetFrameRate = 60),
                    ("30 fps", () => Application.targetFrameRate = 30)
                };
            return result;
        }
        private (string name, Action onOptionSelected)[] GetVSyncOptions(DropdownOption fpsDropdown)
        {
            void SetVSync(int value)
            {
                QualitySettings.vSyncCount = value;
                Debug.Log("QualitySettings.vSyncCount set to " + QualitySettings.vSyncCount);
                
                if (fpsDropdown != null)
                {
                        fpsDropdown.ClearOptions();
                    if (value > 0)
                    {
                        fpsDropdown.AddOption("Auto (controlled by vSync)");
                        fpsDropdown.SetValue(0);
                    }
                    else
                    {
                        fpsDropdown.SetOptions(GetTargetFPSOptions());
                        if (Application.targetFrameRate == -1) fpsDropdown.SetValue(0);
                        else if (Application.targetFrameRate == 60) fpsDropdown.SetValue(1);
                        else if (Application.targetFrameRate == 30) fpsDropdown.SetValue(2);
                    }
                }
                
            }
            var result = new (string name, Action onOptionSelected)[]
                {
                    ("Disabled", () => SetVSync(0)),
                    ("Enabled", () => SetVSync(1))
                };
            return result;
        }

        private (string name, Action onOptionSelected)[] GetDifficultyOptions()
        {
            DifficultyPresetCollection col = DifficultyPresetCollection.DefaultCollection;
            if (col == null)
                return new (string name, Action onOptionSelected)[0];
            int optionCount = col.Presets.Count;
            var result = new (string name, Action onOptionSelected)[optionCount];

            for (int i = 0; i < col.Presets.Count; i++)
            {
                var preset = col.Presets[i];
                result[i] = (preset.name, () => ApplyDifficultyPreset(preset));
            }

            return result;
        }
        public void ApplyDifficultyPreset(DifficultyPreset preset)
        {
            if (preset == null) return;
            var group = GetGroup("Difficulty");
            if (group == null) return;
            foreach (var setting in preset.DifficultySettingList)
            {
                var slider = group.GetOption<SliderOption>(setting.Name);
                if (slider == null) continue;
                slider.SetValue(setting.Value);
            }
        }

        private static (string, Action)[] GetResolutionDropdownItems((string name, int width, int height, FullScreenMode screenMode)[] resolutions)
        {
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

            return resolutionOptions;
        }

        private static (string name, int width, int height, FullScreenMode screenMode)[] GetAvailableResolutions()
        {
            // Only 16:9 resolution options
            return new (string name, int width, int height, FullScreenMode screenMode)[]
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
        }
        #endregion
        #region Utility
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
            RemoveGroup(name);
			var title = Instantiate(MenuNamePrefab, G_MenuNames);

			var group = new SettingsGroup(name, title, G_Options, DropdownPrefab, SliderPrefab, VerticalScrollbarPrefab);
            group.GroupButton.Setup(name, () => EnableGroup(group));

			_availableGroups.Add(group);
			return group;
		}
        void RemoveGroup(string name)
        {
            var group = GetGroup(name);
            if (group == null) return;

            GameObject.Destroy(group.GroupButton.gameObject);
            GameObject.Destroy(group.gameObject);
            _availableGroups.Remove(group);
        }
		SettingsGroup GetGroup(string name)
		{
			foreach (var group in _availableGroups)
				if (group.Name == name)
					return group;
			return null;
		}
        #endregion
    }

}