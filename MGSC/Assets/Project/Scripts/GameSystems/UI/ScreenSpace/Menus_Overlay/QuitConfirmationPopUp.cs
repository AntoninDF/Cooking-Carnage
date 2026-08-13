using GameSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class QuitConfirmationPopUp : UIMenuOverlay
    {
        public Button B_Cancel;
        public Button B_Confirm;
        [Range(0f, 3f)]
        public float DelayBeforeQuit = 1;

        public override void OnLoad()
        {
            base.OnLoad();
            BindAll();
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
            TimerManager.StartTimer("QuitGame", DelayBeforeQuit, QuitGame);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }


    }

}