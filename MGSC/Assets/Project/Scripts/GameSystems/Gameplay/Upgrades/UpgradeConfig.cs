using UnityEngine;

namespace Project.Scripts.GameSystems.Gameplay.Upgrades
{
	/// <summary>
	/// ScriptableObject containing all the variables for an upgrade
	/// </summary>
	[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Config")]
	public class UpgradeConfig : ScriptableObject
	{
		[Tooltip("The base price of this upgrade")]
		public float          basePrice = 100f;
		[Tooltip("The maximum level of the upgrade")]
		public int            maxLevel  = 999;
		[Tooltip("price over the level multiply by the base price")]
		public AnimationCurve priceCurve;
	}
}