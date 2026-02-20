using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{

    public class CookableIngredient : Ingredient
    {
        #region Properties
        public float TargetCookDuration { get=>_targetCookDuration; set=>_targetCookDuration = value; }
        public UnityEvent OnCook => _onCook;
        public float CookDuration { get; set; }
        public float CookPercentage => CookDuration / (TargetCookDuration > 0 ? TargetCookDuration : 1.0f);
        public ECookStatus CookStatus { get; set; }

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
        [Space]
        [Header("Cooking events")]
        [Tooltip("Invoked repeatedly when the ingredient is cooking")]
        [SerializeField] private UnityEvent _onCook;
        #endregion

        #region Methods

        protected override void Start()
        {
            base.Start();
        }

        

        public void Cook(float dT) { CookDuration += dT; OnCook?.Invoke(); }
        public void StopCooking() { }

        #endregion

    }
}
