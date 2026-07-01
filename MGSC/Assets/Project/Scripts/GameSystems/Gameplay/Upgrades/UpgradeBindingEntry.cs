using System;
using Upgrades;

namespace Upgrades
{
	/// <summary>
	/// Class used for reference and associate the SO and the inherited Upgrade class
	/// </summary>
	[Serializable]
	public class UpgradeBindingEntry
	{
		public string        UpgradeTypeName;
		public UpgradeConfig Config;
	}
}