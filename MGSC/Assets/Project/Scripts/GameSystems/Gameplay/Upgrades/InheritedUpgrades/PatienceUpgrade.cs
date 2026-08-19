using UnityEngine;

namespace Upgrades
{
	public class PatienceUpgrade : Upgrade<PatienceUpgrade>
	{
		public float PatienceGainPerLevel => 2f;
		public float GetPatienceGain() => PatienceGainPerLevel * Level;

        public override void LevelUp()
        {
            base.LevelUp();
            if (_level == _config.MaxLevel)
                Achievements.Unlock("ACH_U08");
        }
    }

}