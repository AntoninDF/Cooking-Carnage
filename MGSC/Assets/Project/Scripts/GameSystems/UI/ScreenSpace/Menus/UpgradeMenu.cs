using GameSystem;
using UnityEngine.UI;

namespace UI
{
    public class UpgradeMenu : UIMenu
    {
        public Button B_Confirm;

 
        public override void OnLoad()
        {
            //ShiftManager.OnNextShift += TryShowUpgrades;

            if (B_Confirm != null)
                B_Confirm.onClick.AddListener(TriggerGameStart);
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
        void TriggerGameStart() => Disable();
    }

}