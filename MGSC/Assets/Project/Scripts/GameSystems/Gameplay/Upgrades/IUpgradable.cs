namespace Upgrades
{
	public interface IUpgradable<T> where T : Upgrade
	{
		void OnUpgradeInit(T upgrade);  
		void OnUpgraded(T    upgrade);
	}
}