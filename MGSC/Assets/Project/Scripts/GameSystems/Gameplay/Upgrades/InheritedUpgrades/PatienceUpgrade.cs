using UnityEngine;

namespace Upgrades
{
	public class PatienceUpgrade : Upgrade<PatienceUpgrade>
	{
		public float PatienceGainPerLevel => 2f;
		public float GetPatienceGain() => PatienceGainPerLevel * Level;
	}

}