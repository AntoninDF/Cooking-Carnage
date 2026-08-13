using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SliderOption : UIOption
    {
        public float Value { get => GetValue(); set => SetValue(value);}
        public Vector2 DisplayRange
        {
            get => _displayRange;
            set => SetDisplayRange(value);
        }
        public Vector2 ValueRange = new Vector2(0f, 1f);
        public enum DisplayMode {
            Decimal     = 1 << 0, 
            Integer     = 1 << 1, 
            Percentage  = 1 << 2, 
            Minutes     = 1 << 3
        }
        public DisplayMode CurrentDisplayMode
        {
            get => _currentDisplayMode;
            set => SetCurrentDisplayMode(value);
        }

        [SerializeField] private Slider Slider;
        [SerializeField] private TMP_Text L_Value;
        public Action<float> OnValueChanged = value => { };
        public float defaultValue = 1f;
        public int decimalPrecision = -1;

        private bool integerDisplayMode => (_currentDisplayMode & (DisplayMode.Integer | DisplayMode.Minutes)) != 0;
        private DisplayMode _currentDisplayMode;
        private Vector2 _displayRange = new Vector2(0f, 1f);

        private void Start()
        {
            if (Slider == null) return;
            Slider.onValueChanged.AddListener(OnValueChangedCallback);
        }
        public void Setup(string name, Action<float> onValueChange)
        {
            Name = name;
            OnValueChanged += onValueChange;
        }
        public float GetValue()
        {
            float t = Slider != null ? Slider.value : 0f;
            if (integerDisplayMode)
                t = Mathf.InverseLerp(DisplayRange.x, DisplayRange.y, t);

            float lerpedValue = Mathf.Lerp(ValueRange.x, ValueRange.y, t);
            if (decimalPrecision >= 0 && decimalPrecision <= 10) // If more than 10 digits are required, consider it as if infinite digits are required
            {
                int multiplier = Mathf.RoundToInt(Mathf.Pow(10, decimalPrecision));
                lerpedValue = Mathf.Round(lerpedValue * multiplier) / multiplier;
            }
            return lerpedValue;
        }public float GetDisplayValue()
        {
            float t = Slider != null ? Slider.value : 0f;
            if (integerDisplayMode)
                return t;

            float lerpedValue = Mathf.Lerp(DisplayRange.x, DisplayRange.y, t);
            return lerpedValue;
        }
        public int GetDisplayValueAsInt() => (int)GetDisplayValue();
        /// <summary>
        /// The given value should always be a float between 0 and 1
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(float value)
        {
            if (Slider == null) return;
            float t = Mathf.InverseLerp(ValueRange.x, ValueRange.y, value);
            if (integerDisplayMode)
                t = Mathf.Lerp(DisplayRange.x, DisplayRange.y, t);
            Debug.Log("Set value for " + Name + " : " + value + " with range (" + ValueRange.x + ", " + ValueRange.y + "). t is " + t);
            Slider.value = t;
        }
        public void ResetValue() => SetValue(defaultValue);
        public void OnValueChangedCallback(float value)
        {
            UpdateLabel(value);
            OnValueChanged.Invoke(GetValue());
        }
        private void SetCurrentDisplayMode(DisplayMode value)
        {
            _currentDisplayMode = value;
            if (integerDisplayMode)
            {
                Slider.wholeNumbers = true;
                Slider.minValue = DisplayRange.x;
                Slider.maxValue = DisplayRange.y;
            }
            else
            {
                Slider.wholeNumbers = false;
                Slider.minValue = 0;
                Slider.maxValue = 1;
            }
        }
        private void SetDisplayRange(Vector2 value)
        {
            _displayRange = value;
            if (integerDisplayMode)
            {
                Slider.minValue = value.x;
                Slider.maxValue = value.y;
            }
        }
        public void UpdateLabel(float value)
        {
            if (L_Value == null) return;

            switch (CurrentDisplayMode)
            {
                default:
                case DisplayMode.Decimal:
                    int multiplier = Mathf.RoundToInt(Mathf.Pow(10, Mathf.Min(2, decimalPrecision)));
                    L_Value.text = (Mathf.Round(GetDisplayValue()* multiplier) / multiplier).ToString();
                    break;
                case DisplayMode.Integer:
                    L_Value.text = GetDisplayValueAsInt().ToString();
                    break;
                case DisplayMode.Percentage:
                    L_Value.text = (Mathf.RoundToInt(GetDisplayValue() * 100)).ToString() + "%";
                    break;
                case DisplayMode.Minutes:
                    L_Value.text = GetDisplayValueAsInt().ToString() + "mn";
                    break;
            }
        }
    }

}