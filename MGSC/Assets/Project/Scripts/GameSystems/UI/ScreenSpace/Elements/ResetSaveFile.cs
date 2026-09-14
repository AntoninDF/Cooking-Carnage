using GameSystem;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI
{
	public class ResetSaveFile : UIWidget
	{
		public Button B_Reset;

        private void Start()
        {
            if (B_Reset != null)
                B_Reset.onClick.AddListener(ConfirmReset);
        }

        void ConfirmReset()
        {
            if (!Reference.TryGetManager(out UISystem ui)) return;
            var popup = ui.GetWidget<ConfirmationPopUp>();
            popup.Setup("Are you sure you want to reset your save ? All your progress, upgrades and money will be lost.", ResetSave);
            popup.Enable();
        }


#if UNITY_EDITOR
        [MenuItem("CookingCarnage/Reset save file")]
#endif
        [Button("Reset save file")]
        public static void ResetSave()
        {
            SaveManager.ResetSave();
            SaveManager.LoadGlobalData();
        }
    } 
}
