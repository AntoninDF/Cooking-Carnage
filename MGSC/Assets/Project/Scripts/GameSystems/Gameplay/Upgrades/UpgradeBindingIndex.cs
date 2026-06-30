using System.Collections.Generic;
using UnityEngine;

namespace Upgrades
{
	/// <summary>
	/// Scriptable Singleton that automaticly reference all the inherited Upgrades class
	/// So we can associate the SO config that we want for each of them
	/// </summary>
	[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Binding Index")]
	public class UpgradeBindingIndex : ScriptableObject
	{
		#region Singleton
        
		private static UpgradeBindingIndex _instance;
		public static UpgradeBindingIndex Instance
		{
			get
			{
				if (_instance == null)
					_instance = Resources.Load<UpgradeBindingIndex>("Upgrades/UpgradesBindingIndex");
				if (_instance == null)
					Debug.LogError("[UpgradeBindingIndex] Asset not found in Resources/Upgrades/UpgradesBindingIndex");
				return _instance;
			}
		}
        
		#endregion
		
		#region Fields
        
		[SerializeField] private List<UpgradeBindingEntry> _bindings = new();
		public                   List<UpgradeBindingEntry> Bindings => _bindings;
        
		#endregion
		
		#region Methods
        
		public UpgradeConfig GetConfig(string upgradeTypeName)
		{
			foreach (var entry in _bindings)
				if (entry.UpgradeTypeName == upgradeTypeName)
					return entry.Config;
			return null;
		}
        
		#endregion
	}
}