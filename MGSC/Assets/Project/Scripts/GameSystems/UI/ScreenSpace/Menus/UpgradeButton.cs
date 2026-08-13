using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Upgrades;

namespace UI
{
    public class UpgradeButton : UIWidget
    {
        public bool IsLocked => _config == null ? true : _config.IsLocked;

        public Image I_Icon;
        public Image I_Icon2;
        public TMP_Text L_Title;
        public TMP_Text L_Subtitle;
        public TMP_Text L_PriceTag;
        public Button B_LevelUp;

        public UnityEvent OnLevelUpSuccess;
        public UnityEvent OnLevelUpFail;
        UpgradeConfig _config;

        
        public void Setup(UpgradeConfig config)
        {
            // UnityEngine.Debug.Log($"[UpgradeButton] Setup() appelé, config = {config?.name}, B_LevelUp = {(B_LevelUp != null ? B_LevelUp.name : "NULL")}");

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
            else
                UnityEngine.Debug.LogWarning($"[UpgradeButton] ({name}) B_LevelUp non assigné — le clic ne fera rien");
        }

        public void UpdateGraphics()
        {
            if (_config == null) return;

            if (I_Icon != null && _config.Icon != null)
                I_Icon.sprite = _config.Icon;

            if (I_Icon2 != null && _config.Icon != null)
                I_Icon2.sprite = _config.Icon;

            var upgrade = _config.GetUpgrade();
            if (upgrade == null)
                return;

            // upgrade.GetType().Name can be replaced by _config.name to get the scriptable object's name instead of the type name
            // (And vice versa)
            if (L_Title != null)
                L_Title.text = NicifyName(_config.name);

            if (L_Subtitle != null)
                L_Subtitle.text = "" + upgrade.Level + "/" + _config.MaxLevel;

            if (L_PriceTag != null)
                L_PriceTag.text = upgrade.GetPrice().ToString() + "$";

            if (B_LevelUp.TryGetComponent(out Graphic graphic))
                graphic.color = IsLocked ? new Color(1f,1f,1f, 0.4f) : Color.white;

            // Set as locked visually
            if (!IsLocked) return;
            if (L_Title != null)
                L_Title.text += " (Locked)";
            if (L_PriceTag != null)
                L_PriceTag.text = "?";

        }

        public void RequestLevelUp()
        {
            UnityEngine.Debug.Log($"[UpgradeButton] RequestLevelUp() DÉCLENCHÉ pour {_config?.name}");

            if (_config == null || IsLocked)
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