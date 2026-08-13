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
        public Scrollbar VerticalScrollbarPrefab;
        public List<UIOption> Options = new();
        public ConfigurableButton GroupButton;
        public RectTransform Container;
        public ScrollRect ScrollRect;
        public RectTransform Viewport;
        public bool enabled;

        public SettingsGroup(string name, ConfigurableButton groupButton, Transform contentParent, DropdownOption dropdownPrefab, SliderOption sliderPrefab, Scrollbar verticalScrollbarPrefab)
        {
            Name = name;
            GroupButton = groupButton;
            ContentParent = contentParent;
            DropdownPrefab = dropdownPrefab;
            SliderPrefab = sliderPrefab;
            VerticalScrollbarPrefab = verticalScrollbarPrefab;
            SetupContainerWithScrollView(name);
        }

        public void Enable()
        {
            enabled = true;
            if (ScrollRect != null)
                ScrollRect.gameObject.SetActive(true);
            else
                Container.gameObject.SetActive(true);
        }
        public void Disable()
        {
            enabled = false;
            if (ScrollRect != null)
                ScrollRect.gameObject.SetActive(false);
            else
                Container.gameObject.SetActive(false);
        }
        void SetupContainerWithScrollView(string name)
        {
            if (ScrollRect != null)
                UnityEngine.Object.Destroy(ScrollRect);

            ScrollRect = new GameObject(
                name,
                typeof(ScrollRect))
                .GetComponent<ScrollRect>();
            RectTransform rt = ScrollRect.transform as RectTransform;
            rt.SetParent(ContentParent);
            SetTransformToStretch(rt);

            // Note : The Container requires the Viewport to be correctly setup, and the ScrollRect requires the Container.
            // More over, the scrollbar must be placed after the container in the hierarchy for masking purposes (it could be reordered later, but instantiating them in the right order is easier)
            // so the order of initialization matters

            // Setup viewport rect
            Viewport = new GameObject("Viewport", typeof(RectTransform)).GetComponent<RectTransform>();
            Viewport.SetParent(rt);
            SetTransformToStretch(Viewport);

            
            SetupContainer(name);

            // Setup scrollbar's transform
            Scrollbar sb = GameObject.Instantiate(VerticalScrollbarPrefab.gameObject, rt).GetComponent<Scrollbar>();
            var sbrt = sb.transform as RectTransform;
            sbrt.localScale = Vector3.one;
            sbrt.anchorMin = new Vector2(1, 0);
            sbrt.anchorMax = Vector2.one;
            sbrt.pivot = new Vector2(1, 0.5f);
            sbrt.anchoredPosition = new Vector2(-55, 0);
            sbrt.sizeDelta = new Vector2(20, -100);
            sbrt.localRotation = Quaternion.Euler(0, 0, -3);

            // Setup scrollbar component
            sb.direction = Scrollbar.Direction.BottomToTop;

            // Setup ScrollRect component
            ScrollRect.verticalScrollbar = sb;
            ScrollRect.viewport = Viewport;
            ScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            ScrollRect.content = Container;
            ScrollRect.horizontal = false;

        }
        void SetupContainer(string name)
        {
            if (Container != null)
                UnityEngine.Object.Destroy(Container);

            Container = new GameObject(
                name + " (Container content)",
                typeof(RectTransform))
                .GetComponent<RectTransform>();
            Transform parent = Viewport == null ? ContentParent : Viewport;
            Container.SetParent(parent);
            SetTransformToStretch(Container);

            var layout = Container.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20;
            int padding = 50;
            layout.padding = new RectOffset(padding, padding, padding, padding);
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            // Add an invisible image to get a better pointer raycast target. There perhaps is a cleaner solution to achieve this.
            var img = Container.gameObject.AddComponent<Image>();
            img.color = new(0f, 0f, 0f, 0f);

            UpdatePreferredHeight();
        }

        private void UpdatePreferredHeight()
        {
            int optionHeight = 80;
            int maxOptionCountBeforeOverflow = 6;
            int sizeDeltaY = 0;
            if (Options.Count > maxOptionCountBeforeOverflow)
                sizeDeltaY = (Options.Count - maxOptionCountBeforeOverflow) * optionHeight;
            Container.sizeDelta = new Vector2(Container.sizeDelta.x, sizeDeltaY);
        }

        void SetTransformToStretch(RectTransform rt)
        {
            rt.localScale = Vector2.one;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
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
            UpdatePreferredHeight();

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
        /// <param name="onValueChange">Invoked whenever the slider's value is changed. The float parameter is in the range specified by <paramref name="valueRange"/></param>
        /// <param name="defaultValue">Should be a float between <paramref name="valueRange"/>.x and <paramref name="valueRange"/>.y (default is [0,1]) </param>
        /// <param name="displayMode">How should the value be displayed ?</param>
        /// <param name="displayRange">Specifies the min and max value that will get displayed. By default, it will display values between 0 and 1 (or 0 and 100 for percentage display mode)</param>
        /// <param name="valueRange">Specifies the min and max value for the underlying value that will be passed to the setter.</param>
        /// <param name="decimalPrecision">How many digits after the floating point should be allowed. -1 is limitless. If decimal display mode is selected, it will never show more than 2 decimals</param>
        /// <returns></returns>
        public SettingsGroup AddSlider(
            string name, 
            Action<float> onValueChange, 
            float defaultValue = 0f, 
            SliderOption.DisplayMode displayMode=default, 
            Vector2 displayRange=default, 
            Vector2 valueRange = default,
            int decimalPrecision = -1)
        {
            var prefab = SliderPrefab;
            var option = AddOption(prefab, name);
            if (option == null) return this;

            option.Setup(name, onValueChange);
            option.CurrentDisplayMode = displayMode;
            option.DisplayRange = displayRange == default ? new Vector2(0f, 1f) : displayRange;
            option.ValueRange = valueRange == default ? new Vector2(0f, 1f) : valueRange;
            option.defaultValue = defaultValue;
            option.SetValue(defaultValue);
            option.OnValueChangedCallback(option.GetValue());
            option.decimalPrecision = decimalPrecision;
            return this;
        }
    }
}