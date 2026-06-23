using UnityEditor;
using UnityEngine;

namespace Upgrades
{
	/// <summary>
	/// ScriptableObject that centralizes the configurations for all upgrades.
	/// </summary>
	[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Manager Config")]
	public class UpgradeManagerConfig : ScriptableSingleton<UpgradeManagerConfig>
	{
		[Tooltip("Place the scriptableObject corresponding to the oven upgrade")]
		public UpgradeConfig OvenCount;
		[Tooltip("Place the scriptableObject corresponding to the cooking speed upgrade")]
		public UpgradeConfig CookingSpeed;
		[Tooltip("Place the scriptableObject corresponding to the blender speed upgrade")]
		public UpgradeConfig BlenderSpeed;
		[Tooltip("Place the scriptableObject corresponding to the ingredient drop upgrade")]
		public UpgradeConfig IngredientDrop;


	}
}