namespace Upgrades
{
	public class ExtraDrinkOrderChanceUpgrade : Upgrade<ExtraDrinkOrderChanceUpgrade>
	{
		private const float ChanceGainPerLevel = 0.04f;
		public        float ExtraChanceBonus => _level * ChanceGainPerLevel;
	}
}