namespace Upgrades
{
	public class UnlockDrinkMachine : Upgrade<UnlockDrinkMachine>
	{
        public override void LevelUp()
        {
            base.LevelUp();
            Globals.Authorizations.AllowMakingDrinks.Value = true;
        }
	}
}