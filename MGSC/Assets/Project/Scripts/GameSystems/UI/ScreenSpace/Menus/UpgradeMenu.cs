using GameSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Upgrades;

namespace UI
{
    public class UpgradeMenu : UIMenu
    {
        public override bool ForceLoadOnUIInitialization => true;
        public Button B_NextShift;
        public Button B_MainMenu;
        [Header("Upgrade buttons")]
        public Transform G_UpgradeButtonList;
        public UpgradeButton UpgradeButtonPrefab;
        public ScrollRect ScrollRect;

        private List<UpgradeButton> _upgradeLines = new();
 
        public override void OnLoad()
        {
            base.OnLoad();
            if (B_NextShift != null)
                B_NextShift.onClick.AddListener(TriggerGameStart);

            if (B_MainMenu != null)
                B_MainMenu.onClick.AddListener(OnMainMenu);

            PopulateUpgradeList();

            OnEnable += UpdateGraphicsForEveryUpgrade;
        }

        private void UpdateGraphicsForEveryUpgrade()
        {
            foreach (var line in _upgradeLines)
                line.UpdateGraphics();
        }

        void OnMainMenu()
        {
            GameLoop.RequestTransition(StateTransitions.OpenMainMenu);
        }
        void TryShowUpgrades()
        {
            if (!Reference.TryGetManager(out ShiftManager mgr)) return;
            
            // Don't show upgrades in the tutorial nor at the start of the first shift
            if (mgr.ShiftNumber <= 0)
            {
                TriggerGameStart();
            }
            else
            {
                Enable();
            }
            
        }
        void TriggerGameStart() => GameLoop.RequestTransition(StateTransitions.StartShift);

        void ClearUpgrades()
        {
            if (G_UpgradeButtonList == null)
                return;

            for (int i = G_UpgradeButtonList.childCount-1; i >= 0; i--)
            {
                Transform temp = G_UpgradeButtonList.GetChild(i);
                temp.SetParent(null);
                GameObject.Destroy(temp.gameObject);
            }
            _upgradeLines.Clear();
        }
        void PopulateUpgradeList()
        {
            ClearUpgrades();

            if (G_UpgradeButtonList == null || UpgradeButtonPrefab == null)
                return;

            foreach (var config in GetUpgradeConfigs())
            {
                MakeNewUpgradeButton(config);
            }
        }
        /// <summary>
        ///  Returns the list of all the update configs available
        /// </summary>
        /// <returns></returns>
        IEnumerable<UpgradeConfig> GetUpgradeConfigs() => UpgradeBindingIndex.Instance.Bindings.Select(binding => binding.Config);

        UpgradeButton MakeNewUpgradeButton(UpgradeConfig config)
        {
            if (UpgradeButtonPrefab == null || config == null)
                return null;

            GameObject go = GameObject.Instantiate(UpgradeButtonPrefab.gameObject);
            UpgradeButton result = go.GetComponent<UpgradeButton>();
            go.TryGetComponent(out RectTransform rt);

            go.transform.SetParent(G_UpgradeButtonList);
            go.SetActive(true);
            result.Setup(config);

            rt.localScale = Vector3.one;
            _upgradeLines.Add(result);
            result.OnLevelUpSuccess.AddListener(UpdateGraphicsForEveryUpgrade);

            return result;
        }
    }

}