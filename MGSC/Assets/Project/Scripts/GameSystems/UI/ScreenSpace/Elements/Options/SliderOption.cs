using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SliderOption : UIOption
    {
        public float Value { get => GetValue(); set => SetValue(value);}
        public Vector2 DisplayRange = new Vector2(0f, 1f);
        public Vector2 ValueRange = new Vector2(0f, 1f);
        public enum DisplayMode {Decimal, Integer, Percentage }
        public DisplayMode CurrentDisplayMode;
        [SerializeField] private Slider Slider;
        [SerializeField] private TMP_Text L_Value;
        public Action<float> OnValueChanged = value => { };
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
            float lerpedValue = Mathf.Lerp(ValueRange.x, ValueRange.y, t);
            return lerpedValue;
        }public float GetDisplayValue()
        {
            float t = Slider != null ? Slider.value : 0f;
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
            value = Mathf.InverseLerp(ValueRange.x, ValueRange.y, value);
            Slider.value = value;
        }
        public void OnValueChangedCallback(float value)
        {
            UpdateLabel(value);
            OnValueChanged.Invoke(GetValue());
        }
        public void UpdateLabel(float value)
        {
            if (L_Value == null) return;

            switch (CurrentDisplayMode)
            {
                default:
                case DisplayMode.Decimal:
                    L_Value.text = (Mathf.Round(GetDisplayValue()*100f) /100f).ToString();
                    break;
                case DisplayMode.Integer:
                    L_Value.text = GetDisplayValueAsInt().ToString();
                    break;
                case DisplayMode.Percentage:
                    L_Value.text = ((int)(GetDisplayValue() * 100)).ToString() + "%";
                    break;
            }
        }
    }

}