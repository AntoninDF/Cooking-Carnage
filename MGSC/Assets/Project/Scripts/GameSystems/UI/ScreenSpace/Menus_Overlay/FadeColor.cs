using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class FadeColor : MonoBehaviour
    {

        public float Duration = 1f;
        [SerializeField] private Image _target;
        [SerializeField] private TMP_Text _textTarget;
        [SerializeField] private Color _startColor;
        [SerializeField] private Color _endColor;
        [SerializeField] private UnityEvent _onAnimationEnd;

        private bool _isPlaying;
        private bool _reversed;
        private float _startTime;

        private void Start()
        {
            if (Duration == 0f) Duration = 1f;
        }
        private void Update()
        {

            if ( _isPlaying )
            {
                float t = (Time.time - _startTime);
                if (_reversed) t = Duration - t;
                t /= Duration;
                SetColor(t );
                if (t >= 1f)
                {
                    _isPlaying = false;
                    _onAnimationEnd?.Invoke();
                }
            }
        }
        public void Play()
        {
            _reversed = false;
            _startTime = Time.time;
            _isPlaying = true;
        }
        public void PlayReversed()
        {
            _reversed = true;
            _startTime = Time.time;
            _isPlaying = true;
        }

        private void SetColor(float t)
        {
            t = Mathf.Clamp01(t);

            if (_target != null)
                _target.color = Color.Lerp(_startColor, _endColor, t);
            
            if (_textTarget != null)
                _textTarget.color = Color.Lerp(_startColor, _endColor, t);

        }
    }
}