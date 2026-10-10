using GameSystem;
using Orders;
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

            onEnable += UpdateGraphicsForEveryUpgrade;
            onEnable += () => PlayTransition("Start of shift " + Globals.Gameplay.ShiftNumber);
        }

        private void UpdateGraphicsForEveryUpgrade()
        {
            CheckUpgradeLocks();
            foreach (var line in _upgradeLines)
                line.UpdateGraphics();
            ScrollRect.verticalNormalizedPosition = 0;
            ScrollRect.verticalScrollbar.value = 1;
            Debug.Log("[UpgradeMenu] UpdateGraphicsForEveryUpgrade() : Menu should be scrolled to the top");
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
        void CheckUpgradeLocks()
        {
            foreach (var line in _upgradeLines)
            {
                if (line.Config.GetUpgrade().GetType() == typeof(MixtRecipePityThresholdUpgrade))
                {
                    var book = Reference.AssetReferences.DefaultRecipeBook;
                    var recipe = book.Recipes.Where(recipe => RecipeCategoryHelper.GetCategory(recipe) == RecipeCategory.MixteTriple).FirstOrDefault();
                    line.Config.IsLocked = !RecipeUnlockChecker.IsUnlocked(recipe);
                }
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

            var configs = GetUpgradeConfigs();
            // Sort configs so that le locked ones are showing last (-1 means "show before" and 1 means "show after")
            configs.Sort(new Comparison<UpgradeConfig>((x, y) =>
                {
                    if (x == null) return 1;
                    if (y == null) return -1;
                    if (x.IsLocked == y.IsLocked) return 0;
                    if (x.IsLocked) return 1;
                    return -1;
                }
                ));
            foreach (var config in configs)
            {
                MakeNewUpgradeButton(config);
            }
        }
        /// <summary>
        ///  Returns the list of all the update configs available
        /// </summary>
        /// <returns></returns>
        List<UpgradeConfig> GetUpgradeConfigs() => UpgradeBindingIndex.Instance.Bindings.Select(binding => binding.Config).ToList();

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