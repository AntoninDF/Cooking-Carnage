using TMPro;
using UnityEngine;

namespace Localization
{
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        #region Properties

        #endregion

        #region Fields


        private TMP_Text _textComponent;
        #endregion

        #region Methods
        private bool RequireInit()
        {
            return _textComponent == null;
        }
        private void Init()
        {
            _textComponent = GetComponent<TMP_Text>();
        }
        #endregion
    }

}