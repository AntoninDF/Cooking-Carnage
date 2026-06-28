using GameSystem;
using System.Collections;
using UnityEngine;

namespace UI
{

    public class FadingTooltip : Tooltip
    {
        #region Fields
        [Header("Fading parameters")]
        [SerializeField] private float _timeBeforeFading = 5f;
        [SerializeField] private float _duration = 3f;


        #endregion
        private void Start()
        {
            transform.localScale = Vector3.zero;
        }


        public void EnableAndFadeOut()
        {
            transform.localScale = Vector3.one;
            TimerManager.StartTimer("Fading tooltip", _timeBeforeFading, () => StartCoroutine(Shrink(_duration)));
        }

        IEnumerator Shrink(float duration)
        {
            if (duration <= 0.0f)
            {
                transform.localScale = Vector3.zero;
                yield break;
            }
            for (float t = 0; t < _duration; t += Time.deltaTime)
            {
                transform.localScale = Vector3.one * (1 - t/duration);
                yield return null;
            }
        }
    }

}