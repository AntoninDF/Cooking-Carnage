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
            EnforceDatabaseSetup(Database);
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

            _textComponent.text = GetText(Database, ID, Language);
        }

        public static string GetText(LocalizedTextDatabase db, string ID,  AvailableLanguages Language)
        {
            if (db == null)
                return "";
            if (!db.IsSetup)
                Debug.LogWarning("[LocalizedText] : The database doesn't seem to be setup properly");
            string text = "";

            if (!string.IsNullOrEmpty(ID))
            {
                text = db.Get(ID, Language);
            }

            return text;
        }

        public static void EnforceDatabaseSetup(LocalizedTextDatabase Database)
        {
            if (Database != null && !Database.IsSetup)
                Database.Load();
        }
        #endregion
    }

}