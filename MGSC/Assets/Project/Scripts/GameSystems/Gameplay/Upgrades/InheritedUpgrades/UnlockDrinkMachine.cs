namespace Upgrades
{
	public class UnlockDrinkMachine : Upgrade<UnlockDrinkMachine>
	{
        public override void LevelUp()
        {
            base.LevelUp();
            Globals.Authorizations.AllowMakingDrinks.Value = true;
        }
        public override void SetLevel(int level)
        {
            base.SetLevel(level);
            Globals.Authorizations.AllowMakingDrinks.Value = _level > 0;
        }
    }
}