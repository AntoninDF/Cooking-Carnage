using GameSystem;
using NaughtyAttributes;
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
        //public TMP_Text L_PreviousTotalMoney;
        public TMP_Text L_CurrentTotalMoney;
        public TMP_Text L_MoneyDelta;
        [SerializeField] private Color _defaultColor = Color.gray2;
        [SerializeField] private Color _failureColor = Color.firebrick;
        [SerializeField] private Color _successColor = Color.seaGreen;

        [SerializeField] private Action onTotalMoneyBegin;
        [SerializeField] private Action onTotalMoneyEnd;

        public Transform G_ReceiptLines;
        public ShiftRecapLine ReceiptLinePrefab;

        [Tooltip("The gameobjects listed here will be enabled when the shift has failed, and disabled otherwise")]
        public List<GameObject> ObjectsToEnableUponFailOnly = new();
        [Tooltip("The gameobjects listed here will be enabled when the shift has failed, and disabled otherwise")]
        public List<GameObject> ObjectsToEnableUponSuccessOnly = new();
        [Tooltip("Each image listed here will have the correct sprite assigned wether the shift has succeeded or failed")]
        public List<S_SpriteSwap> SpriteSwaps = new();
        public UnityEvent OnFail;

        private List<ShiftRecapLine> _currentLines = new();

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
            
            //SetPreviousTotalMoney(mm.GetMoneyOnPreviousShift());

            if (sm.HasFailed)
                SetContentMode(ContentMode.Fail);
            else
                SetContentMode(ContentMode.Success);

            if (L_CurrentTotalMoney != null)
                L_CurrentTotalMoney.text = "...";

            AnimateReceipt(
                UpdateTotalMoney
                );

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
        [Button("Animate Receipt")]
        void AnimateReceipt(Action onEnd=null)
        {
            if (!Reference.TryGetManager(out MoneyManager mm)) return;
            if (!Reference.TryGetManager(out ShiftManager sm)) return;

            // Remove existing lines
            _currentLines.Clear();
            if (G_ReceiptLines == null) return;
            for (int i = 0; i < G_ReceiptLines.childCount; i++)
                GameObject.Destroy(G_ReceiptLines.GetChild(i).gameObject);

            AnimateRecipeLine(
                "Rent payed",
                -sm.GetCurrentRentPrice(),
            onEnd:
            () => AnimateRecipeLine(
                "Upgrades",
                -(mm.GetMoneyLostThisShift() + sm.GetCurrentRentPrice()),
            onEnd:
            () => AnimateRecipeLine(
                "Salary",
                mm.GetMoneyGainedThisShift(),
                onEnd
                )
                )
                )
                ;
        }
        void AnimateRecipeLine(string name, float targetValue, Action onEnd=null)
        {
            var go = Instantiate(ReceiptLinePrefab.gameObject, G_ReceiptLines);
            ShiftRecapLine line = go.GetComponent<ShiftRecapLine>();
            line.SetLabel(name);
            _currentLines.Add(line);
            line.OnValueChange += UpdateMoneyDelta;
            line.Animate(targetValue, onEnd);
        }
        private void UpdateMoneyDelta()
        {
            if (L_MoneyDelta == null) return;
            float value = 0;
            foreach(var line in _currentLines)
            {
                value += line.Value;
            }
            L_MoneyDelta.text = (value > 0 ? "+" : "") + value + "$";
            if (value > 0)
                L_MoneyDelta.color = _successColor;
            else if (value < 0)
                L_MoneyDelta.color = _failureColor;
            else if (value == 0)
                L_MoneyDelta.color = _defaultColor;


            if (!Reference.TryGetManager(out MoneyManager mm)) return;
        }
        [Button("Animate Money")]
        void UpdateTotalMoney()
        {
            onTotalMoneyBegin?.Invoke();
            if (!Reference.TryGetManager(out MoneyManager mm)) return;
            if (!Reference.TryGetManager(out ShiftManager sm)) return;

            string timerName = "Animate total money";
            float startValue = mm.GetMoneyAtTheBeginningOfThisShift();
            float endValue = mm.MoneyInfo;

            L_CurrentTotalMoney.color = _defaultColor;

            Action onEnd =
                () =>
                {
                    L_CurrentTotalMoney.color =
                        endValue > startValue ? _successColor :
                        endValue > 0 ? _defaultColor :
                        _failureColor;
                    TimerManager.RemoveTimer(timerName);
                    L_CurrentTotalMoney.text = endValue + "$";
                    onTotalMoneyEnd?.Invoke();
                };
            Action<float> onUpdate =
                t =>
                {
                    float lerpedValue = Mathf.Round(Mathf.Lerp(startValue, endValue, t));
                    L_CurrentTotalMoney.text = lerpedValue + "$";
                };

            float duration = 1f;
            TimerManager.StartTimer(
                timerName,
                duration,
                onEnd,
                elapsed => onUpdate(elapsed / duration));
        }

        void SetText(ref TMP_Text txt, float value)
        {
            if (txt != null)
                txt.text = ((int)value).ToString();
        }
        void SetMoneyGained(float value)    => SetText(ref L_MoneyGained, value);
        void SetRentPaid(float value)       => SetText(ref L_RentPaid, value);
        //void SetPreviousTotalMoney(float value)   => SetText(ref L_PreviousTotalMoney, value);
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