using NaughtyAttributes;
using UnityEngine;

namespace Upgrades
{
	/// <summary>
	/// ScriptableObject containing all the variables for an upgrade
	/// </summary>
	[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Config")]
	public class UpgradeConfig : ScriptableObject
	{
		[Expandable]
		[Tooltip("The maximum level of the upgrade")]
		public int            MaxLevel  = 999;
		
		[Expandable]
		[Tooltip("price over the level multiply by the base price")]
		public AnimationCurve PriceCurve;
	}

	
}