using GameSystem;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

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

        [Button("Reset save file")]
        public void ResetSave()
        {
            SaveManager.ResetSave();
            SaveManager.LoadGlobalData();
        }
    } 
}
