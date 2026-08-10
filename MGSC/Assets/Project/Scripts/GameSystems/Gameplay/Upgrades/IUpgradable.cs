namespace Upgrades
{
	/// <summary>
	/// Interface used by a script to access an upgrade with the UpgradeManager as a facade
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IUpgradable<T> where T : Upgrade<T>
	{
		void OnUpgradeInit(T upgrade);  
		void OnUpgraded(T    upgrade);
        public  void TrySubscribe()
        {
            if (Reference.TryGetManager(out UpgradeManager mgr))
                mgr.Subscribe(this as IUpgradable<T>);
        }
    }
}