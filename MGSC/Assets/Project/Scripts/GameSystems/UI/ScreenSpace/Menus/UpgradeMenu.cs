using GameSystem;
using UnityEngine.UI;

namespace UI
{
    public class UpgradeMenu : UIMenu
    {
        public Button B_NextShift;
        public Button B_MainMenu;

 
        public override void OnLoad()
        {
            if (B_NextShift != null)
                B_NextShift.onClick.AddListener(TriggerGameStart);

            if (B_MainMenu != null)
                B_MainMenu.onClick.AddListener(OnMainMenu);
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
    }

}