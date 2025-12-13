using TMPro;
using UnityEngine;

namespace Localization
{
    [ExecuteAlways]
    [AddComponentMenu("Localization/LocalizedText")]
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        #region Properties

        #endregion

        #region Fields
        public LocalizedTextDatabase Database;
        public string ID;
        public AvailableLanguages Language = AvailableLanguages.Auto;

        private TMP_Text _textComponent;
        #endregion

        #region Methods

        private void Start()
        {
            Init();
            UpdateText();
        }

        private bool RequireInit()
        {
            return _textComponent == null;
        }
        private void Init()
        {
            _textComponent = GetComponent<TMP_Text>();
        }

        public void UpdateText()
        {
            if (Database == null)
                return;
            if (RequireInit()) Init();
            if (!Database.IsSetup)
                Debug.LogWarning("[LocalizedText] : The database doesn't seem to be setup properly");
            string text = _textComponent.text;

            if (!string.IsNullOrEmpty(ID))
            {
                text = Database.Get(ID, Language);
            }

            _textComponent.text = text;
        }

        #endregion
    }

}