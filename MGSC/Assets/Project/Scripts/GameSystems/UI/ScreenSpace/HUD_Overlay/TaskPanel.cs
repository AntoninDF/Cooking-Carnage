using GameSystem;
using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class TaskPanel : UIHUDOverlay
	{
		[SerializeField, Required] private TaskItem _taskPrefab;
		[SerializeField, Required] private Transform G_TaskList;


		#region Methods

		public Action BindTask(string label)
		{
			var task = MakeNewTask(label);

			return task.RequestResolve;
		}

		[Button("Make test task")]
		public void Test01()
			=> ShiftManager.OnShiftBegin +=
				BindTask("Start the shift by pulling on the rope (Lorem ipsum dolor sit amet)");
		public TaskItem MakeNewTask(string label) => _taskPrefab?.MakeTaskBySelfCopy(label, G_TaskList);

        [ContextMenu("Refresh layouts")]
        public void RefreshContentFitters() => RefreshContentFitters(transform as RectTransform);
        public static void RefreshContentFitters(RectTransform transform)
        {
            RefreshContentFitter(transform);
        }

        private static void RefreshContentFitter(RectTransform transform)
        {
            if (transform == null || !transform.gameObject.activeSelf)
            {
                return;
            }

            foreach (RectTransform child in transform)
            {
                RefreshContentFitter(child);
            }

            var layoutGroup = transform.GetComponent<LayoutGroup>();
            var contentSizeFitter = transform.GetComponent<ContentSizeFitter>();
            if (layoutGroup != null)
            {
                layoutGroup.SetLayoutHorizontal();
                layoutGroup.SetLayoutVertical();
            }

            if (contentSizeFitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            }
        }
        #endregion
    }

}