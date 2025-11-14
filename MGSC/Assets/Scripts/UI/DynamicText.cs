using GameSystem;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;


namespace UI
{

    public class DynamicText : MonoBehaviour
    {
        #region Properties

        #endregion

        #region Fields

        [Header("References")]
        [Tooltip("The target text object's to update")]
        [SerializeField] private TMP_Text _textObject;
        [Tooltip("This is the text that will be displayed. Place the markups in it for them to get replaced by actual data")]
        [SerializeField] private string _baseText;
        [Tooltip("Each element specifies which markup to replace with which value")]
        [SerializeField] private List<UIDataReplacement> _dataReplacement = new();

        #endregion

        #region Methods
        private void Start()
        {
            if (_textObject == null) _textObject = GetComponent<TMP_Text>();
        }
        private void Update()
        {
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (_textObject == null) return;
            DataManager dataManager = ReferenceManager.Instance.GetManager<DataManager>();
            if (dataManager == null)
                Debug.LogError("[ShiftRecap] : Couldn't update the UI, no DataManager found");
            string result = string.Copy(_baseText);
            foreach (var textInfo in _dataReplacement)
            {
                if (textInfo.markup == "") continue;
                result = result.Replace(textInfo.markup, dataManager.GetDataAsString(textInfo.info));
            }
            _textObject.text = result;
            Debugger.Instance.Log("Data replacement is ", _dataReplacement);
        }

        #endregion


        [System.Serializable]
        public struct UIDataReplacement
        {
            [Tooltip("This text will be replaced by the requested info type")]
            public string markup;
            public EDataInfo info;

            public override string ToString()
            {
                return markup + " as " + info.ToString();
            }
        }

    }

}