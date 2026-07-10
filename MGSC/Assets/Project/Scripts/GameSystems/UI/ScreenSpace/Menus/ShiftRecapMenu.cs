using GameSystem;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class ShiftRecapMenu : UIMenu
    {
        public override bool ForceLoadOnUIInitialization => true;
        public Button B_NextShift;
        public Button B_MainMenu;

        public TMP_Text L_MoneyGained;
        public TMP_Text L_RentPaid;
        public TMP_Text L_PreviousTotalMoney;
        public TMP_Text L_CurrentTotalMoney;

        [Tooltip("The gameobjects listed here will be enabled when the shift has failed, and disabled otherwise")]
        public List<GameObject> ObjectsToEnableUponFailOnly = new();
        [Tooltip("The gameobjects listed here will be enabled when the shift has failed, and disabled otherwise")]
        public List<GameObject> ObjectsToEnableUponSuccessOnly = new();
        [Tooltip("Each image listed here will have the correct sprite assigned wether the shift has succeeded or failed")]
        public List<S_SpriteSwap> SpriteSwaps = new();
        public UnityEvent OnFail;

        public override void OnLoad()
        {
            if (Reference.TryGetManager(out MoneyManager mgr))
            {
                mgr.OnMoneyChange += SetMoneyGained;
            }
            OnEnable += UpdateTexts;
            SetContentMode(ContentMode.Default);
            BindAll();
        }

        void BindAll()
        {
            if (B_NextShift != null)
                B_NextShift.onClick.AddListener(OnNextShift);
            if (B_MainMenu != null)
                B_MainMenu.onClick.AddListener(OnMainMenu);
        }

        void OnMainMenu()
        {
            GameLoop.RequestTransition(StateTransitions.OpenMainMenu);
        }

        void OnNextShift()
        {
            GameLoop.RequestTransition(StateTransitions.UpgradeShop);
        }
        void UpdateTexts()
        {
            if (!Reference.TryGetManager(out MoneyManager mm)) return;
            if (!Reference.TryGetManager(out ShiftManager sm)) return;
            SetMoneyGained(mm.GetMoneyGainedThisShift());
            SetCurrentTotalMoney(mm.MoneyInfo);
            SetPreviousTotalMoney(mm.GetMoneyOnPreviousShift());

            if (sm.HasFailed)
                SetContentMode(ContentMode.Fail);
            else
                SetContentMode(ContentMode.Success);
        }
        void SetContentMode(ContentMode mode = ContentMode.Default)
        {
            if (mode == ContentMode.Fail)
                OnFail?.Invoke();

            if (B_NextShift != null)
                B_NextShift.gameObject.SetActive(mode != ContentMode.Fail);

            foreach (var go in ObjectsToEnableUponFailOnly)
                go.SetActive(mode == ContentMode.Fail);

            foreach (var go in ObjectsToEnableUponSuccessOnly)
                go.SetActive(mode == ContentMode.Success);

            foreach (var ss in SpriteSwaps)
                if (ss.Target != null)
                    ss.Target.sprite = mode switch
                    {
                        ContentMode.Default => ss.Default,
                        ContentMode.Success => ss.Success,
                        ContentMode.Fail    => ss.Fail,
                        _                   => ss.Default
                    };
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

        [System.Serializable]
        public struct S_SpriteSwap
        {
            public Image Target;
            public Sprite Default;
            public Sprite Success;
            public Sprite Fail;
        }
        public enum ContentMode { Default, Success, Fail}

        private void OnValidate()
        {
            for (int i = 0; i < SpriteSwaps.Count; i++)
            {
                S_SpriteSwap ss = SpriteSwaps[i];
                if (ss.Target != null && ss.Default == null)
                    ss.Default = ss.Target.sprite;
                SpriteSwaps[i] = ss;
            }

        }
    }

}