using GameSystem;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : UIMenu
    {

        #region Fields
        [Tooltip("This is the play button, it will start the game")]
        public Button B_Play;
        [Tooltip("This is the settings button, it will open the settings menu")]
        public Button B_Settings;
        [Tooltip("This is the credits button, it will start the credits")]
        public Button B_Credits;
        [Tooltip("This is the quit button, it will quit the game")]
        public Button B_Quit;

        [Tooltip("These objects will be disabled unless we're in the release version")]
        [SerializeField] List<GameObject> _releaseSpecificObjects; 
        [Tooltip("These objects will be disabled unless we're in the demo version")]
        [SerializeField] List<GameObject> _demoSpecificObjects; 
        #endregion


        #region Methods
        public override void OnLoad()
        {
            base.OnLoad();
            BindAll();
            LoadSpecificObjects();
        }

        private void LoadSpecificObjects()
        {
#if !DEMO
            // Release mode
            foreach (GameObject obj in _releaseSpecificObjects)
                obj.SetActive(true);
            foreach (GameObject obj in _demoSpecificObjects)
                obj.SetActive(false);
#else
            // Demo mode
            foreach (GameObject obj in _demoSpecificObjects)
                obj.SetActive(true);
            foreach (GameObject obj in _releaseSpecificObjects)
                obj.SetActive(false);
#endif
        }

        void BindAll()
        {
            if (B_Play != null)
                B_Play.onClick.AddListener(OnPlay);
            
            if (B_Settings != null)
                B_Settings.onClick.AddListener(OnSettings);
            
            if (B_Credits != null)
                B_Credits.onClick.AddListener(OnCredits);
            
            if (B_Quit != null)
                B_Quit.onClick.AddListener(OnQuit);
        }

        void OnPlay()
        {
            GameLoop.RequestTransition(StateTransitions.StartShift);
        }

        private void OnSettings()
        {
            if (Reference.TryGetManager(out UISystem ui))
            {
                var menu = ui.GetWidget<SettingsMenu>();
                menu?.Enable();
            }
        }

        void OnCredits()
        {
            GameLoop.RequestTransition(StateTransitions.OpenCredits);
        }

        void OnQuit()
        {
            var menu = Owner.GetWidget<QuitConfirmationPopUp>();
            menu.Enable();
        }


#endregion
    } 
}
