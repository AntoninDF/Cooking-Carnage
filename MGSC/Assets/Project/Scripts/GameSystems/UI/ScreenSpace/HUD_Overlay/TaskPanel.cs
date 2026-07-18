using GameSystem;
using NaughtyAttributes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class TaskPanel : UIHUDOverlay
	{
		public List<TaskItem> ActiveTasks
		{
			get => _activeTasks;
		}
		[SerializeField, Required] private TaskItem _taskPrefab;
		[SerializeField, Required] private Transform G_TaskList;

		private List<TaskItem> _activeTasks = new();
		#region Methods

		/// <summary>
		/// This generates a task to show up
		/// </summary>
		/// <param name="label"></param>
		/// <param name="subscribe"></param>
		/// <param name="unsubscribe"></param>
		public TaskItem BindTask(string label, Action<Action> subscribe, Action<Action> unsubscribe, Action onResolve = null)
		{
			var task = MakeNewTask(label);
            _activeTasks.Add(task);
			EnableIfActiveTask();
            Debug.Log("Binding task");
			subscribe(task.RequestResolve);
			task.OnResolve += () =>
			{
				unsubscribe(task.RequestResolve);
				_activeTasks.Remove(task);
				EnableIfActiveTask();
            }; 
			task.OnResolve += onResolve;
			return task;
		}
		private TaskItem MakeNewTask(string label) => _taskPrefab?.MakeTaskBySelfCopy(label, G_TaskList);
		private void EnableIfActiveTask()
		{
			if (_activeTasks.Count > 0)
				Owner.ShowWidget(this);
			else
				Owner.StashWidget(this);
		}
        
        #endregion
    }

}