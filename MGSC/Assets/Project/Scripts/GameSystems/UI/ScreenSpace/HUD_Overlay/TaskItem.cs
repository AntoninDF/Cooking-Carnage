using GameSystem;
using NaughtyAttributes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(LayoutElement), typeof(ContentSizeFitter), typeof(CanvasGroup))]
    public class TaskItem : UIWidget
    {
        public string Label
        {
            get => L_Label.text;
            set => L_Label.text = value;
        }
        [SerializeField, Required] private TMP_Text L_Label;
        [SerializeField] private float _timeBetweenResolutionAndDestruction = 1f;
        [SerializeField] private UnityEvent OnSetup;
        [SerializeField] private UnityEvent OnResolutionFeedbacks;
        public Action OnResolve;
        public Action OnResolveFeedbacksOver;
        private Action _unbindCallback;

        List<Timer> _timers = new();
        #region Methods

        private void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.update += RefreshLayout;
#endif
        }
        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= RefreshLayout;
#endif
        }

        /// <summary>
        /// Treats this TaskItem as a template and duplicates it to a new task
        /// </summary>
        /// <param name="label"></param>
        public TaskItem MakeTaskBySelfCopy(string label, Transform parent = null)
        {
            GameObject go = GameObject.Instantiate(gameObject, parent);
            TaskItem task = go.GetComponent<TaskItem>();

            task.Setup(label);

            return task;
        }

        /// <summary>
        /// Setup this task to bind itself to the validator event, display the label, and resolve upon validator invocation.
        /// </summary>
        /// <param name="Label"></param>
        /// <param name="validatorEvent"></param>
        public void Setup(string label)
        {
            Label = label;
            RefreshLayout();
            OnSetup?.Invoke();
        }

        public void SetUnbindingOperation (Action action)
        {
            _unbindCallback = action;
        }
        public void RequestResolve()
        {
            Debug.Log("Request resolve for '" + Label + "'");
            OnResolve?.Invoke();
            PlayResolutionFeedbacks();
            float delay = _timeBetweenResolutionAndDestruction;
            if (delay <= 0)
                Resolve();
            else
                CallDelayedResolve(delay);
        }
        void PlayResolutionFeedbacks() => OnResolutionFeedbacks?.Invoke();
        private void Resolve()
        {
            foreach (var timer in _timers)
                TimerManager.RemoveTimer(timer.Name);
            OnResolveFeedbacksOver?.Invoke();
            Destroy(gameObject);
        }

        private void CallDelayedResolve(float delay)
            => _timers.Add(TimerManager.StartTimer("Resolve task " + Time.time, delay, Resolve, elapsed => Debug.Log("Resolving task... " + elapsed + "/" + delay + "s")));

        private void RefreshLayout()
        {
            var layout = GetComponent<LayoutElement>();
            if (L_Label == null) return;
            layout.preferredHeight = L_Label.preferredHeight;

            foreach (var child in transform)
            {
                RectTransform rt = child as RectTransform;
                layout.preferredHeight = Mathf.Max(layout.preferredHeight, rt.sizeDelta.y);
            }

            if (transform.parent != null && transform.parent.TryGetComponent(out LayoutGroup parent))
                parent.SetLayoutVertical();
        }

        public void FadeIn(float duration)
        {
            var canvas = GetComponent<CanvasGroup>();
            var c = AnimationCurve.EaseInOut(0, 0, 1, 1);
            _timers.Add(TimerManager.StartTimer("Fade in taskitem " + Time.time + GetHashCode(), duration, null, elapsed => canvas.alpha = c.Evaluate(elapsed / duration)));
        }
        public void FadeOut(float duration)
        {
            var canvas = GetComponent<CanvasGroup>();
            var c = AnimationCurve.EaseInOut(0, 0, 1, 1);
            _timers.Add(TimerManager.StartTimer("Fade out taskitem " + Time.time + GetHashCode(), duration, null, elapsed => canvas.alpha = 1 - c.Evaluate(elapsed / duration)));
        }
        #endregion
    }

}