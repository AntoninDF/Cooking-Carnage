using Orders;
using Upgrades;

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