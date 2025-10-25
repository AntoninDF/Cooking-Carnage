using TMPro;
using UnityEngine;

namespace UI
{
    public class Tooltip : MonoBehaviour
    {
        #region Properties

        #endregion

        #region Fields
        [Header("Tooltip references")]
        [Tooltip("Text component to update")]
        [SerializeField] private TMP_Text _textComponent;
        #endregion

        #region Methods

        public void SetText(string text)
        {
            EnforceReferences();
            _textComponent.text = text;
        }

        private void EnforceReferences()
        {
            if (_textComponent == null)
            {
                Debug.LogError("[Tooltip] (" + name + ") : Please reference a text component in the inspector");
            }
        }
        #endregion
    }
}