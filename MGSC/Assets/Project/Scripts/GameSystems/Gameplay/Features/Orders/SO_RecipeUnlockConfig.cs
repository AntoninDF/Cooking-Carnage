using System;
using System.Collections.Generic;
using UnityEngine;

namespace Orders
{
	[CreateAssetMenu(fileName = "RecipeUnlockConfig", menuName = "Scriptable Objects/Orders/Recipe Unlock Config")]
	public class SO_RecipeUnlockConfig : ScriptableObject
	{
		
		#region Singleton
        
		private static SO_RecipeUnlockConfig _instance;
		public static SO_RecipeUnlockConfig Instance
		{
			get
			{
				if (_instance == null)
					_instance = Resources.Load<SO_RecipeUnlockConfig>("Orders/RecipeUnlockConfig");
				if (_instance == null)
					Debug.LogError("[UpgradeBindingIndex] Asset not found in Resources/Orders/RecipeUnlockConfig");
				return _instance;
			}
		}
        
		#endregion
		
		
		[Serializable]
		public struct UnlockRequirement
		{
			public RecipeCategory category;
			public int             minShift;
			public float           minMoney;
		}

		public List<UnlockRequirement> requirements = new();

		public bool TryGetRequirement(RecipeCategory category, out UnlockRequirement requirement)
		{
			foreach (var r in requirements)
			{
				if (r.category == category)
				{
					requirement = r;
					return true;
				}
			}
			requirement = default;
			return false;
		}
	}
}