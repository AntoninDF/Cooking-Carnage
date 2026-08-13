using UnityEngine;

namespace Upgrades
{
	public class MixtRecipePityThresholdUpgrade : Upgrade<MixtRecipePityThresholdUpgrade>
	{
		public int DefaultPityThreshold => 15;
		public int DecrementPityThresholdPerLevel => 1;
		public int CurrentPityThreshold => Mathf.Max(0, DefaultPityThreshold - DecrementPityThresholdPerLevel * Level);
	}

}