using System.Collections.Generic;
using System;
using UI;
using UnityEngine.UI;
using UnityEngine;


namespace UI
{
    public class SettingsGroup
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
                UnityEngine.Object.Destroy(Container);

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

            Options.Add(option);

            return option;
        }

        public SettingsGroup AddDropdown(string name, (string name, Action onOptionSelected)[] options, int defaultValue = 0)
        {
            var prefab = DropdownPrefab;
            var option = AddOption(prefab, name);
            if (option == null) return this;
            option.ClearOptions();

            option.SetOptions(options);
            option.SetValue(Math.Min(defaultValue, options.Length - 1));

            return this;
        }
        public SettingsGroup AddDropdown<EnumType>(string name, params Action[] callbacks) where EnumType : Enum
        {
            var prefab = DropdownPrefab;
            var option = AddOption(prefab, name);
            if (option == null) return this;
            option.ClearOptions();

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
        public SettingsGroup AddDropdown<EnumType>(string name, Action<int> onChange, EnumType defaultValue = default) where EnumType : Enum
        {
            var prefab = DropdownPrefab;
            var option = AddOption(prefab, name);
            if (option == null) return this;
            option.ClearOptions();

            string[] names = Enum.GetNames(typeof(EnumType));

            for (int i = 0; i < names.Length; i++)
            {
                option.AddOption(names[i]);
            }
            option.OnValueChanged += onChange;
            option.SetValue(defaultValue.GetHashCode());

            return this;
        }

        /// <summary>
        /// Adds a slider to this group after all the previously added sliders
        /// </summary>
        /// <param name="name">The name will be displayed in the menu</param>
        /// <param name="onValueChange">Invoked whenever the slider's value is changed. The float parameter is in the range specified by <paramref name="displayRange"/></param>
        /// <param name="defaultValue">Should be a float between 0 and 1</param>
        /// <param name="displayMode"></param>
        /// <param name="displayRange"></param>
        /// <returns></returns>
        public SettingsGroup AddSlider(string name, Action<float> onValueChange, float defaultValue = 0f, SliderOption.DisplayMode displayMode=default, Vector2 displayRange=default, Vector2 valueRange = default)
        {
            var prefab = SliderPrefab;
            var option = AddOption(prefab, name);
            if (option == null) return this;

            option.Setup(name, onValueChange);
            option.CurrentDisplayMode = displayMode;
            option.DisplayRange = displayRange == default ? new Vector2(0f, 1f) : displayRange;
            option.ValueRange = valueRange == default ? new Vector2(0f, 1f) : valueRange;
            option.SetValue(defaultValue);
            option.OnValueChangedCallback(option.GetValue());
            return this;
        }
    }
}