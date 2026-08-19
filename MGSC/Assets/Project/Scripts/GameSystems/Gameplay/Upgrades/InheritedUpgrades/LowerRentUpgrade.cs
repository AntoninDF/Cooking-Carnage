using UnityEngine;
namespace Upgrades
{
	public class LowerRentUpgrade : Upgrade<LowerRentUpgrade>
	{
		public float DiscountInPercentagePerLevel => 0.025f;
		public float GetRentMultiplier() => Mathf.Max(0f, 1f - Level * DiscountInPercentagePerLevel);
        public override void LevelUp()
        {
            base.LevelUp();
            if (_level == _config.MaxLevel)
                Achievements.Unlock("ACH_U07");
        }
    }

}