using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SliderOption : UIOption
    {
        [SerializeField] private Slider Slider;
        public Action<float> OnValueChanged = value => { };
        private void Start()
        {
            if (Slider == null) return;
            Slider.onValueChanged.AddListener(OnValueChanged.Invoke);
        }
        public void Setup(string name, Action<float> onValueChange)
        {
            Name = name;
            OnValueChanged += onValueChange;
        }
        public float GetValue() => Slider != null ? Slider.value : 0f;
        public void SetValue(float value)
        {
            if (Slider != null)
                Slider.value = value;
        }
    }

}