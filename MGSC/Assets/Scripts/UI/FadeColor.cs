using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class FadeColor : MonoBehaviour
    {

        [SerializeField] private Image _target;
        [SerializeField] private Color _startColor;
        [SerializeField] private Color _endColor;
        [SerializeField] private float _duration;
        [SerializeField] private UnityEvent _onAnimationEnd;

        private bool _isPlaying;
        private bool _reversed;
        private float _startTime;

        private void Start()
        {
            if (_duration == 0f) _duration = 1f;
        }
        private void Update()
        {

            if ( _isPlaying )
            {
                float t = (Time.time - _startTime);
                if (_reversed) t = _duration - t;
                t /= _duration;
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
            if (_target == null)
                Debug.LogError("FadeColor target was null.");
            t = Mathf.Clamp01( t );
            _target.color = Color.Lerp(_startColor, _endColor, t);
        }
    }
}