using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using Upgrades;

namespace UI
{
    public class UpgradeButton : UIWidget
    {
        public Image I_Icon;
        public TMP_Text L_Title;
        public TMP_Text L_Subtitle;
        public TMP_Text L_PriceTag;
        public Button B_LevelUp;

        public UnityEvent OnLevelUpSuccess;
        public UnityEvent OnLevelUpFail;
        UpgradeConfig _config;

        
        public void Setup(UpgradeConfig config)
        {
            BindWithUpgrade(config);
            UpdateGraphics();

            OnLevelUpFail.AddListener(UpdateGraphics);
            OnLevelUpSuccess.AddListener(UpdateGraphics);
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

            // upgrade.GetType().Name can be replaced by _config.name to get the scriptable object's name instead of the type name
            if (L_Title != null)
                L_Title.text = NicifyName(upgrade.GetType().Name);

            if (L_Subtitle != null)
                L_Subtitle.text = "" + upgrade.Level + "/" + _config.MaxLevel;

            if (L_PriceTag != null)
                L_PriceTag.text = upgrade.GetPrice().ToString() + "$";
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

        string NicifyName(in string name)
        {
            string temp = name.Replace("Upgrade", "");
            temp = temp.Replace("_", " ");
            string result = "";
            for (int i = 0; i < temp.Length; i++)
            {
                char c = temp[i];
                if (i == 0)
                {
                    result += c;
                    continue;
                }
                if (char.IsUpper(c) && char.IsLower(temp[i - 1]))
                    result += " ";
                result += c;
            }
            return result;
        }
    }

}