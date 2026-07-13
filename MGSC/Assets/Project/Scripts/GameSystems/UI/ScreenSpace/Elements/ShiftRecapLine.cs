using GameSystem;
using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class ShiftRecapLine : UIWidget
    {
        public Action OnValueChange;
        public float Value;

        [SerializeField] private TMP_Text L_Label;
        [SerializeField] private TMP_Text L_Value;
        [SerializeField] private Color _defaultColor = Color.gray2;
        [SerializeField] private Color _failureColor = Color.firebrick;
        [SerializeField] private Color _successColor = Color.seaGreen;
        [SerializeField] private bool _applyColorToLabel;
        [SerializeField] private bool _applyOpacityToLabel = true;
        [SerializeField] private AnimationCurve _heightInterpolation = AnimationCurve.EaseInOut(0,0,0.2f,1);
        [SerializeField] private AnimationCurve _opacityInterpolation = AnimationCurve.EaseInOut(0.2f,0,0.4f,1);
        [SerializeField] private AnimationCurve _numberInterpolation = AnimationCurve.Linear(0.4f,0,1,1);
        [SerializeField] private float _duration = 2f;

        private float _defaultHeight;
        private float _targetValue;
        private Timer _currentTimer;

        ~ShiftRecapLine() => ClearTimer();
        public void SetLabel(string name)
        {
            if (L_Label != null)
                L_Label.text = name;
        }
        public void SetValue(float value, float opacity=1f)
        {
            Value = value;
            if (L_Value != null)
            {
                L_Value.text = value > 0 ? "+" : "" + value.ToString() + "$";

                if (value > 0)
                    L_Value.color = _successColor;
                else if (value < 0)
                    L_Value.color = _failureColor;
                else if (value == 0)
                    L_Value.color = _defaultColor;
                var c = L_Value.color;
                L_Value.color = new Color(c.r, c.g, c.b, c.a * opacity);
            }
            if (L_Label != null && _applyColorToLabel)
            {
                if (value > 0)
                    L_Label.color = _successColor;
                else if (value < 0)
                    L_Label.color = _failureColor;
                else if (value == 0)
                    L_Label.color = _defaultColor;
            }
            if (L_Label != null && _applyOpacityToLabel)
            {
                if (!_applyColorToLabel)
                    L_Label.color = _defaultColor;
                var c = L_Label.color;
                L_Label.color = new Color(c.r, c.g, c.b, c.a * opacity);
            }
            OnValueChange?.Invoke();
        }


        public void Animate(float targetValue, Action onEnd)
        {
            _targetValue = targetValue;
            _defaultHeight = GetComponent<RectTransform>().sizeDelta.y;
            onEnd += () => Animate(1);
            ClearTimer();
            _currentTimer = TimerManager.StartTimer(
                "Animate receipt line " + Time.time,
                _duration,
                onEnd,
                elapsed => Animate(elapsed / _duration, true));
        }

        private void ClearTimer()
        {
            if (_currentTimer != null)
            {
                _currentTimer.Pause();
                TimerManager.RemoveTimer(_currentTimer.Name);
            }
        }

        /// <summary>
        /// t is between 0 and 1
        /// </summary>
        /// <param name="t"></param>
        private void Animate(float t, bool roundValue=false)
        {
            var rt = GetComponent<RectTransform>();
            var t1 = _heightInterpolation.Evaluate(t);
            var t2 = _numberInterpolation.Evaluate(t);
            var t3 = _opacityInterpolation.Evaluate(t);

            rt.sizeDelta = new Vector2(rt.sizeDelta.x, t1 * _defaultHeight);
            float value = roundValue ? Mathf.Round(_targetValue * t2) : _targetValue * t2;
            SetValue(value, t3);
        }
    }

}