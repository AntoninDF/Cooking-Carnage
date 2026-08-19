using Assets.Scripts.UI;
using GameSystem;
using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ConfirmationPopUp : UIMenuOverlay
    {
        public TMP_Text L_Text;
        public Button B_Cancel;
        public Button B_Confirm;
        public FadeColor FadeObject;

        Action _onConfirm;
        float _defaultTextSize;
        bool keepOpen;

        public override void OnLoad()
        {
            base.OnLoad();
            BindAll();
            if (L_Text != null) _defaultTextSize = L_Text.fontSize;
        }

        public void Setup(string text, Action onConfirm, float textSize=-1, bool keepMenuOpenAfterConfirmation = false)
        {
            if (L_Text != null)
            {
                L_Text.text = text;
                if (textSize != -1)
                    L_Text.fontSize = textSize;
                else
                    L_Text.fontSize = _defaultTextSize;
            }
            _onConfirm = onConfirm;
            keepOpen = keepMenuOpenAfterConfirmation;
        }

        void BindAll()
        {
            if (B_Cancel != null)
                B_Cancel.onClick.AddListener(OnCancel);

            if (B_Confirm != null)
                B_Confirm.onClick.AddListener(OnConfirm);
        }

        void OnCancel()
        {
            Disable();
        }
        void OnConfirm()
        {
            _onConfirm?.Invoke();
            if (!keepOpen)
                Disable();
        }
        public void QuitGameWithDelay_1s() => QuitGameWithDelay(1);
        public void QuitGameWithDelay(float delay=1f)
        {
            FadeObject.gameObject.SetActive(true);
            FadeObject.Duration = delay;
            FadeObject.Play();
            TimerManager.StartTimer("QuitGame", delay, QuitGame);
        }
        public static void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }


    }

}