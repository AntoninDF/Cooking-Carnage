namespace Upgrades
{
	public static class UpgradableExtensions
	{
		public static void TrySubscribe<T>(this IUpgradable<T> self) where T : Upgrade<T>
		{
			if (Reference.TryGetManager(out UpgradeManager mgr))
				mgr.Subscribe(self);
		}
	}
}