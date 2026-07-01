using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [HideScriptField]
    public class MainMenu : UIMenu
    {
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
        }

        void Reset()
        {
            B_Play = gameObject.FindAnyObjectByName<Button>("B_Play");
            B_Credits = gameObject.FindAnyObjectByName<Button>("B_Credits");
            B_Quit = gameObject.FindAnyObjectByName<Button>("B_Quit");
        }


        #endregion
    } 
}
