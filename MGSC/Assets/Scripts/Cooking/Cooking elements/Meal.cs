using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// Defines the data and behaviour needed to, uh, be a meal
    /// </summary>
    public class Meal : Item, ITrashable, ICookable
    {
        #region Properties
        public SO_Recipe Recipe { get => _recipe; set => _recipe = value; }
        public float TargetCookDuration { get => _targetCookDuration; set => _targetCookDuration = value; }
        public float CookDuration { get; set; }

        public float CookPercentage => CookDuration / (TargetCookDuration > 0 ? TargetCookDuration : 1.0f); // Between 0 and 1

        public ECookStatus CookStatus { get; set; } = ECookStatus.Cooked;

        public UnityEvent<ICookable> OnCook => _onCook;
        #endregion

        #region Fields
        [Header("Meal references")]
        [Tooltip("Set what recipe this meal represents")]
        [SerializeField] private SO_Recipe _recipe;
        [Header("Plate events")]
        [Tooltip("Invoked repeatedly when the plate is cooking")]
        [SerializeField] private UnityEvent<ICookable> _onCook;

        private float _targetCookDuration = 0.0f;
        #endregion

        #region Methods

        protected override void Start()
        {
            base.Start();
            _tooltipComponent.SetText(Recipe.Name);
        }

        public void Cook(float dT)
        {
            _targetCookDuration = _recipe.timeToOvercook;
            CookDuration += dT;
            OnCook?.Invoke(this);
            UpdateTooltip();

            if (CookPercentage >= 1)
            {
                TransformToBurntMeal();
            }
        }
        private void TransformToBurntMeal()
        {
            name = name + "(burnt)";
            CookStatus = ECookStatus.Overcooked;
        }
        public void StopCooking()
        {
            throw new System.NotImplementedException();
        }
        private void UpdateTooltip()
        {
            string text;
            if (CookDuration == 0)
            {
                text = "Plate";
            }
            else
            {
                text = "Cooking (" + Mathf.RoundToInt(CookPercentage * 100 + 100) + "%)";
            }
            if (_tooltipComponent != null)
            {
                _tooltipComponent.SetText(text);
            }
        }
        #endregion
    }

}