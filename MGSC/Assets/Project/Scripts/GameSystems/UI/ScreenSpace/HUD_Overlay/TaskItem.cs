using NaughtyAttributes;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
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

        }


        public void Resolve()
        {

            OnResolve?.Invoke();
        }
    }

}