using UnityEngine;

namespace Upgrades
{
	/// <summary>
	/// ScriptableObject containing all the variables for an upgrade
	/// </summary>
	[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Config")]
	public class UpgradeConfig : ScriptableObject
	{
		[Tooltip("The maximum level of the upgrade")]
		public int            MaxLevel  = 999;
		[Tooltip("price over the level multiply by the base price")]
		public AnimationCurve PriceCurve;
	}

	
}