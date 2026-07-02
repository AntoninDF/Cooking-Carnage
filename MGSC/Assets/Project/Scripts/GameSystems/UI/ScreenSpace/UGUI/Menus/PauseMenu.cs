using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class PauseMenu : UIMenu
    {
        public override bool ForceLoadOnUIInitialization => true;
        public Button B_Resume;
        public Button B_MainMenu;
        public Button B_Quit;

        public override void OnLoad()
        {
            GameLoop.OnResume += Disable;
            BindEscapeKey();
            BindButtons();
        }

        private void BindButtons()
        {
            if (B_Resume != null)
                B_Resume.onClick.AddListener(OnResume);
            if (B_MainMenu != null)
                B_MainMenu.onClick.AddListener(OnMainMenu);
            if (B_Quit != null)
                B_Quit.onClick.AddListener(OnQuit);
        }
        void OnResume()
        {
            GameLoop.TryResume(this);
        }
        void OnMainMenu()
        {
            GameLoop.TryResume(this);
            var menu = Owner.GetWidget<MainMenu>();
            menu.Enable();
            Disable();
        }
        void OnQuit()
        {
            var menu = Owner.GetWidget<QuitConfirmationPopUp>();
            menu.Enable();
        }


        private void BindEscapeKey()
        {
            var actions = Reference.AssetReferences.InputActions;
            var inpt = actions.FindAction("Pause");
            if (inpt == null) return;
            inpt.performed += TryTogglePause;
            return;
        }

        void TryTogglePause(InputAction.CallbackContext temp)
        {
            if (Enabled == false)
            {
                // Do not enable the pause menu if the game is already paused
                if (!GameLoop.IsRunning) return;
                GameLoop.RequestPause(this);
                Enable();
            }
            else
            {
                GameLoop.TryResume(this);
            }
        }
    }

}