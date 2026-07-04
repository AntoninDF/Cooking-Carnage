using System;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using Upgrades;
using static UnityEngine.Rendering.STP;

namespace UI
{
    public class UpgradeButton : UIWidget
    {
        public Image I_Icon;
        public Button B_LevelUp;
        public TMP_Text L_PriceTag;
        public TMP_Text L_UnlockedUpgrades;

        public UnityEvent OnLevelUpSuccess;
        public UnityEvent OnLevelUpFail;
        UpgradeConfig _config;

        public void Setup(UpgradeConfig config)
        {
            BindWithUpgrade(config);
            UpdateGraphics();
        }
        public void BindWithUpgrade(UpgradeConfig config)
        {
            _config = config;
            if (config == null)
            {
                UnityEngine.Debug.LogWarning($"[UpgradeButton] ({name}) The given config was null");
                return;
            }

            if (B_LevelUp != null)
                B_LevelUp.onClick.AddListener(RequestLevelUp);
        }

        public void UpdateGraphics()
        {
            if (_config == null) return;

            if (I_Icon != null)
                I_Icon.sprite = _config.Icon;

            var upgrade = _config.GetUpgrade();
            if (upgrade == null)
                return;

            if (L_PriceTag != null)
                L_PriceTag.text = upgrade.GetPrice().ToString();

            if (L_UnlockedUpgrades != null)
                L_UnlockedUpgrades.text = "" + upgrade.Level + "/" + _config.MaxLevel;
        }

        void RequestLevelUp()
        {
            if (_config == null)
            {
                OnLevelUpFail?.Invoke();
                return;
            }
            if (!Reference.TryGetManager(out UpgradeManager mgr))
            {
                OnLevelUpFail?.Invoke();
                return;
            }

            if (!mgr.TryLevelUp(_config))
            {
                OnLevelUpFail?.Invoke();
                return;
            }
            else
            {
                OnLevelUpSuccess?.Invoke();
                return;
            }
        }

    }

}