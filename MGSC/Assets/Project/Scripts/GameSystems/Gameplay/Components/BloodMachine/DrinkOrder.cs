using Orders;
using Upgrades;

namespace Project.Scripts.BloodMachine
{
	public class DrinkOrder : IUpgradable<ExtraDrinkOrderChanceUpgrade>
	{
		public void OnUpgradeInit(ExtraDrinkOrderChanceUpgrade upgrade)
		{
			OrderPicker.SetExtraDrinkChanceBonus(upgrade.ExtraChanceBonus);
		}

		public void OnUpgraded(ExtraDrinkOrderChanceUpgrade upgrade)
		{
			OrderPicker.SetExtraDrinkChanceBonus(upgrade.ExtraChanceBonus);
		}
	}
}