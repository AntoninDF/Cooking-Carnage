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

		/// <summary>
		/// This generates a task to show up
		/// </summary>
		/// <param name="label"></param>
		/// <param name="subscribe"></param>
		/// <param name="unsubscribe"></param>
		public TaskItem BindTask(string label, Action<Action> subscribe, Action<Action> unsubscribe, Action onResolve = null)
		{
			var task = MakeNewTask(label);
			Debug.Log("Binding task");
			subscribe(task.RequestResolve);
			task.OnResolve += () => unsubscribe(task.OnResolve);
			task.OnResolve += onResolve;
			return task;
		}
		[Button("Make test task")]
		public void Test01()
			=> BindTask(
				"Earn some money by serving a client", 
				e => MoneyManager.OnMoneyGain += e, 
				e => MoneyManager.OnMoneyGain -= e,
				() => Debug.Log("Task resolved"));

		private TaskItem MakeNewTask(string label) => _taskPrefab?.MakeTaskBySelfCopy(label, G_TaskList);

        
        #endregion
    }

}