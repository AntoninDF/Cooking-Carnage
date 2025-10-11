using System;
using TMPro;
using UnityEngine;

namespace Cooking
{

    public class CookableIngredient : Ingredient, ICookable
    {
        #region Properties
        public float TargetCookDuration { get=>_targetCookDuration; set=>_targetCookDuration = value; }
        public Action<ICookable> OnCook { get; set; }
        public float CookDuration { get; set; }
        public float CookPercentage => CookDuration / (TargetCookDuration > 0 ? TargetCookDuration : 1.0f);

        #endregion

        #region Fields
        [Header("Cooking references")]
        [Tooltip("Reference the text component that indicates how much the ingredient is cooked")]
        [SerializeField] private TMP_Text _cookIndicator;
        [Tooltip("Put the following markup to replace it by the percentage : {0}")]
        [SerializeField] private string _cookIndicatorText;
        [Space]
        [Header("Cooking parameters")]
        [Tooltip("How long should this object be cooking (in seconds) before it is 100% cooked")]
        [SerializeField] private float _targetCookDuration = 10f;
        #endregion

        #region Methods

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            UpdateUI();
        }

        protected override void UpdateUI()
        {
            base.UpdateUI();
            if (_cookIndicator == null)
                throw new MissingReferenceException("Please reference the cook indicator in the inspector of " + name);
            _cookIndicator.text = string.Format(_cookIndicatorText, Mathf.Round(CookPercentage * 100) + "%");
        }

        #endregion

    }
}
