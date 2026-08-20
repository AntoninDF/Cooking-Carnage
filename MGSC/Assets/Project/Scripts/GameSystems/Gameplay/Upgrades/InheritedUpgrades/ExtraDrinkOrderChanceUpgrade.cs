namespace Upgrades
{
	public class ExtraDrinkOrderChanceUpgrade : Upgrade<ExtraDrinkOrderChanceUpgrade>
	{
		private const float ChanceGainPerLevel = 0.04f;
		public        float ExtraChanceBonus => _level * ChanceGainPerLevel;
        public override void LevelUp()
        {
            base.LevelUp();
			Globals.Gameplay.DrinkProbabilityIncrease.Value = ExtraChanceBonus;
        }
	}
}