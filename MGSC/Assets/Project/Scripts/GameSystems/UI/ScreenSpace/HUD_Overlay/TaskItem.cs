using GameSystem;
using NaughtyAttributes;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(LayoutElement), typeof(ContentSizeFitter))]
    public class TaskItem : UIWidget
    {
        public string Label
        {
            get => L_Label.text;
            set => L_Label.text = value;
        }
        [SerializeField, Required] private TMP_Text L_Label;
        [SerializeField] private float _timeBetweenResolutionAndDestruction = 1f;
        [SerializeField] private UnityEvent OnResolve;

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
        }


        public void RequestResolve()
        {
            float delay = _timeBetweenResolutionAndDestruction;
            if (delay <= 0)
                Resolve();
            else
                CallDelayedResolve(delay);
        }
        private void Resolve()
        {
            OnResolve?.Invoke();
        }
        private void CallDelayedResolve(float delay)
            => TimerManager.StartTimer("Resolve task " + Time.time, delay, Resolve);

        private void RefreshLayout()
        {
            if (L_Label == null) return;
            var layout = GetComponent<LayoutElement>();
            layout.preferredHeight = L_Label.preferredHeight;
        } 
        #endregion
    }

}