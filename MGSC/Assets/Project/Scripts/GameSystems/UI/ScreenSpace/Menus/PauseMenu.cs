using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class PauseMenu : UIMenu
    {
        public override bool ForceLoadOnUIInitialization => true;
        public Button B_Resume;
        public Button B_MainMenu;
        public Button B_Settings;
        public Button B_Quit;

        public override void OnLoad()
        {
            base.OnLoad();
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
            if (B_Settings != null)
                B_Settings.onClick.AddListener(OnSettings);
            if (B_Quit != null)
                B_Quit.onClick.AddListener(OnQuit);
        }
        void OnResume()
        {
            TryTogglePause(default);
        }
        void OnMainMenu()
        {
            var menu = Owner.GetWidget<ConfirmationPopUp>();
            menu.Enable();
            menu.Setup("Are you sure you want to go back to the main menu ?<br><color=#900>Progress made this shift won't be saved", GoToMainMenu, 50);

        }
        void GoToMainMenu()
        {
            Disable();
            GameLoop.RequestTransition(StateTransitions.OpenMainMenu);
        }
        void OnSettings()
        {
            Owner.GetWidget<SettingsMenu>().Enable();
        }
        void OnQuit()
        {
            var menu = Owner.GetWidget<ConfirmationPopUp>();
            menu.Enable();
            menu.Setup("Are you sure you want to quit ?", menu.QuitGameWithDelay_1s, keepMenuOpenAfterConfirmation:true);
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
                AudioSystem.PlayAudio(AudioSystem.Database.pauseOpen);
            }
            else
            {
                if (Owner.GetShownWidgets<UIMenuOverlay>().Count == 0)
                {
                    GameLoop.TryResume(this);
                    AudioSystem.PlayAudio(AudioSystem.Database.pauseClose);
                }
            }
        }
    }

}