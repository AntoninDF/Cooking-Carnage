using GameSystem;
using NUnit.Framework;
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

 
        public override void OnLoad()
        {
            if (B_NextShift != null)
                B_NextShift.onClick.AddListener(TriggerGameStart);

            if (B_MainMenu != null)
                B_MainMenu.onClick.AddListener(OnMainMenu);

            PopulateUpgradeList();
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

            for (int i = G_UpgradeButtonList.childCount; i > 0; i--)
            {
                Debug.Log("Destroying " + G_UpgradeButtonList.GetChild(0).name);
                GameObject.Destroy(G_UpgradeButtonList.GetChild(0).gameObject);
            }
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

            go.transform.SetParent(G_UpgradeButtonList);
            go.SetActive(true);
            result.Setup(config);

            return result;
        }
    }

}