using UnityEngine;
using Upgrades;

namespace Upgrades
{
	public class DrinkServingSpeedUpgrade : Upgrade<DrinkServingSpeedUpgrade>
	{
		private const float BaseFlowRatePerHalfSecond = 3f;   
		private const float FlowRateGainPerLevel      = 0.7f; 

		public float FlowRatePerHalfSecond => BaseFlowRatePerHalfSecond + _level * FlowRateGainPerLevel;
		public float FlowRatePerSecond     => FlowRatePerHalfSecond * 2f;

		public float GetServiceDuration(float drinkQuantityCl, float minDuration = 0.1f)
		{
			if (FlowRatePerSecond <= 0f) return minDuration;
			return Mathf.Max(minDuration, drinkQuantityCl / FlowRatePerSecond);
		}
	}
}