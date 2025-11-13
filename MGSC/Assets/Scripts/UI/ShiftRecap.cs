using GameSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace UI
{

    public class ShiftRecap : MonoBehaviour
    {
        #region Properties

        #endregion

        #region Fields

        [Header("References")]
        [Tooltip("")]
        [SerializeField] private List<UITextElement> _textObjects = new();

        #endregion

        #region Methods

        private void Update()
        {
            UpdateUI();
        }

        public void UpdateUI()
        {
            DataManager dataManager = ReferenceManager.Instance.GetManager<DataManager>();
            if (dataManager == null)
                Debug.LogError("[ShiftRecap] : Couldn't update the UI, no DataManager found");
            foreach (var textInfo in _textObjects)
            {
                if (textInfo.textObject == null) continue;
                if (textInfo.text == "") continue;
                textInfo.textObject.text = textInfo.text.Replace( "{0}", dataManager.GetDataAsString(textInfo.info));
            }
        }

        #endregion


        [System.Serializable]
        public struct UITextElement
        {
            public TMP_Text textObject;
            public string text;
            public EDataInfo info;
        }

    }

}