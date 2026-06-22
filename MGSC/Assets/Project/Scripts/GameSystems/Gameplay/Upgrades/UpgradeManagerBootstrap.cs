using UnityEngine;
using Upgrades;

namespace Project.Scripts.GameSystems.Gameplay.Upgrades
{
	public class UpgradeManagerBootstrap : MonoBehaviour
	{
		#region Fields

		[SerializeField] private UpgradeManagerConfig _config;

		#endregion


		#region UnityLifecycle

		private void Awake()
		{
			if (_config == null)
			{
				Debug.LogError("UpgradeManagerConfig is not assigned");
				return;
			}
			Reference.GetManager(out UpgradeManager _upgradeManager);
			if (_upgradeManager == null)
			{
				Debug.LogError("No UpgradeManager found in Reference");
			}
			
			_upgradeManager.SetConfig(_config);
		}
		

		#endregion
	}
}