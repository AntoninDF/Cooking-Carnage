using GameSystem;
using System;
using TMPro;
using UnityEngine.UI;

namespace UI
{
    public class ShiftRecapMenu : UIMenu
    {
        public override bool ForceLoadOnUIInitialization => true;
        public Button B_NextShift;
        private Button B_MainMenu;

        public TMP_Text L_MoneyGained;
        public TMP_Text L_RentPaid;
        public TMP_Text L_PreviousTotalMoney;
        public TMP_Text L_CurrentTotalMoney;


        public Action TestAction;

        public override void OnLoad()
        {
            ShiftManager.OnShiftEnded += Enable;
            if (Reference.TryGetManager(out MoneyManager mgr))
            {
                mgr.OnMoneyChange += SetMoneyGained;
            }
            OnEnable += UpdateTexts;
            BindAll();
        }

        void BindAll()
        {
            if (B_NextShift != null)
                B_NextShift.onClick.AddListener(OnNextShift);
        }

        void OnNextShift()
        {
            if (Reference.TryGetManager(out ShiftManager mgr))
            {
                mgr.RequestNextShift();
            }
            Disable();
        }
        void UpdateTexts()
        {
            if (!TryGetComponent(out MoneyManager mm)) return;
            if (!TryGetComponent(out ShiftManager sm)) return;
            SetMoneyGained(mm.GetMoneyGainedThisShift());
            SetCurrentTotalMoney(mm.MoneyInfo);
            // Update other texts as well
        }
        void SetText(ref TMP_Text txt, float value)
        {
            if (txt != null)
                txt.text = ((int)value).ToString();
        }
        void SetMoneyGained(float value)    => SetText(ref L_MoneyGained, value);
        void SetRentPaid(float value)       => SetText(ref L_RentPaid, value);
        void SetPreviousTotalMoney(float value)   => SetText(ref L_PreviousTotalMoney, value);
        void SetCurrentTotalMoney(float value)   => SetText(ref L_CurrentTotalMoney, value);
    }

}