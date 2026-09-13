using GameSystem;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CreditsMenu : UIMenu
    {
        public Button B_Back;

        [Header("Scrolling settings")]
        public bool EnableScrolling = true;
        [Tooltip("Useful only if you want scrolling credits"), ShowIf("EnableScrolling")]
        public ScrollRect ScrollView;
        [Tooltip("Unit is 'pixels per seconds', based on a Full-HD pixel size"), Min(1f), ShowIf("EnableScrolling")]
        public float ScrollSpeed = 300;
        [Tooltip("The duration of the credits' scroll, in seconds"), Min(1f), ShowIf("EnableScrolling")]
        public float ScrollDuration = 120;
	[ShowIf("EnableScrolling")]
	public float DelayStartScrolling;
        [ShowIf("EnableScrolling")]
        [ReadOnly] public Mask ViewportMask;
        public float ScrollSpeedMultiplier { get; set; } = 1f;

        private bool isScrolling = false;
        float _smoothingFactor;
        float _previousPos;
        private float previousDuration = 120;
        public override void OnLoad()
        {
            base.OnLoad();
            BindAll();

            ViewportMask.enabled = true;
            if (EnableScrolling)
                onEnable += () => ScrollDelayed(DelayStartScrolling);
        }

        private void BindAll() => B_Back?.onClick.AddListener(OnBack);
        void OnBack() => GameLoop.RequestTransition(StateTransitions.OpenMainMenu);

        #region Scrolling

        void Update()
        {
            ProcessElapsedTime();
        }
        void TryStartScrolling()
        {
            if (!Enabled) return;
            if (ScrollView == null) return;

            ScrollView.verticalNormalizedPosition = 1;
            _previousPos = 1;
            _smoothingFactor = 0f;
            isScrolling = true;
        }
        void ProcessElapsedTime()
        {
            if (!isScrolling) return;
            if (ScrollView == null) return;

            // Get the delta we should add to the scrollbar to get a scrolling effect
            float delta = 1f / ScrollDuration * Time.deltaTime * ScrollSpeedMultiplier * _smoothingFactor;
            if (_smoothingFactor < 1) _smoothingFactor += Mathf.Min(1 - _smoothingFactor, Time.deltaTime / 3f);

            var pos = ScrollView.verticalNormalizedPosition;
            if (_previousPos != pos)
            {
                PauseScroll();
                TimerManager.StartTimer("Paused credits scrolling due to user input", 0.05f, ResumeScroll);
                _previousPos = pos;
                return;
            }
            ScrollView.verticalNormalizedPosition = pos - delta;
            _previousPos = ScrollView.verticalNormalizedPosition;

            //Debug.Log("Credits Y pos is " + ScrollView.verticalNormalizedPosition);
            if (ScrollView.verticalNormalizedPosition <= 0.001f)
                OnScrollEnd();
        }

        void OnScrollEnd()
        {
            isScrolling = false;
            ScrollView.verticalNormalizedPosition = 0f;
            _previousPos = 0;
            ScrollDelayed(5f);
        }
        void ScrollDelayed(float delay = 1f) => TimerManager.StartTimer("Wait before starting credits' scroll", delay, TryStartScrolling);
        void PauseScroll()
        {
            isScrolling = false;
            _smoothingFactor = 0f;
        }
        void ResumeScroll() => isScrolling = true;

        void OnEnable() => previousDuration = ScrollDuration;
        void OnValidate()
        {
            RectTransform content = null;
            if (ScrollView != null)
            {
                content = ScrollView.content;
                ViewportMask = ScrollView.viewport.GetComponent<Mask>();
            }

            if (content != null)
            {
                if (previousDuration != ScrollDuration)
                {
                    previousDuration = ScrollDuration;
                    ScrollSpeed = content.sizeDelta.y / ScrollDuration;
                }
                else
                {
                    ScrollDuration = content.sizeDelta.y / ScrollSpeed;
                    previousDuration = ScrollDuration;
                }
            }
        }
        #endregion
    }
}