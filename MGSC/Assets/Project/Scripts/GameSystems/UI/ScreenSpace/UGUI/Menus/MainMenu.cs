using GameSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : UIMenu
    {
        public override bool ForceLoadOnUIInitialization => true;

        #region Fields
        [Tooltip("This is the play button, it will start the game")]
        public Button B_Play;
        [Tooltip("This is the credits button, it will start the credits")]
        public Button B_Credits;
        [Tooltip("This is the quit button, it will quit the game")]
        public Button B_Quit;


        #endregion


        #region Methods
        public override void OnLoad()
        {
            base.OnLoad();
            Enable();
            BindAll();
        }

        void BindAll()
        {
            if (B_Play != null)
                B_Play.onClick.AddListener(OnPlay);
            
            if (B_Credits != null)
                B_Credits.onClick.AddListener(OnCredits);
            
            if (B_Quit != null)
                B_Quit.onClick.AddListener(OnQuit);
        }
        void OnPlay()
        {
            Disable();
            if (Reference.TryGetManager(out ShiftManager mgr))
            {
                mgr.RequestNextShift();
            }
        }

        void OnCredits()
        {
            var menu = Owner.GetWidget<CreditsMenu>();
            menu.Enable();
        }

        void OnQuit()
        {
            var menu = Owner.GetWidget<QuitConfirmationPopUp>();
            menu.Enable();
        }


        #endregion
    } 
}
