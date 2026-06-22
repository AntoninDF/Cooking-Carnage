using UnityEngine;

namespace Project.Scripts.GameSystems.Gameplay.Upgrades
{
	/// <summary>
	/// ScriptableObject that centralizes the configurations for all upgrades.
	/// </summary>
	[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Manager Config")]
	public class UpgradeManagerConfig : ScriptableObject
	{
		[Tooltip("Place the scriptableObject corresponding to the oven upgrade")]
		public UpgradeConfig ovenCount;
		[Tooltip("Place the scriptableObject corresponding to the cooking speed upgrade")]
		public UpgradeConfig cookingSpeed;
		[Tooltip("Place the scriptableObject corresponding to the blender speed upgrade")]
		public UpgradeConfig blenderSpeed;
		[Tooltip("Place the scriptableObject corresponding to the ingredient drop upgrade")]
		public UpgradeConfig ingredientDrop;
	}
}