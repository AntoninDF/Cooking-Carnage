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
                B_Reset.onClick.AddListener(ResetSave);
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
