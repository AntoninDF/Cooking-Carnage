using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// Defines the data and behaviour needed to, uh, be a meal
    /// </summary>
    public class Meal : Item
    {
        #region Properties
        public SO_Recipe Recipe { get => _recipe; set => _recipe = value; }
        public float TargetCookDuration { get => _targetCookDuration; set => _targetCookDuration = value; }
        public float CookDuration { get; set; }

        public float CookPercentage => CookDuration / (TargetCookDuration > 0 ? TargetCookDuration : 1.0f); // Between 0 and 1

        public ECookStatus CookStatus { get; set; }  = ECookStatus.Cooked;

        public UnityEvent OnCook => _onCook;
        public UnityEvent OnOvercooked => _onOvercooked;
        
        
        #endregion

        #region Fields
        
        [SerializeField] string _itemID;
        
        [Header("Meal references")]
        [Tooltip("Set what recipe this meal represents")]
        [SerializeField] private SO_Recipe _recipe;
        [Header("Meal events")]
        [Tooltip("Invoked repeatedly when the meal is cooking")]
        [SerializeField] private UnityEvent _onCook;
        [Tooltip("Invoked when the meal gets overcooked")]
        [SerializeField] private UnityEvent _onOvercooked;

        private float _targetCookDuration = 0.0f;
        public bool eaten;
        #endregion

        #region Methods

        protected override void Awake()
        {
            base.Awake();
            if (Recipe == null) Debug.LogWarning("Recipe field was not set to an instance of an object");
            

            OnCook?.Invoke();
        }
        
        public  void Reset()
        {
            eaten = false;
            CookStatus = ECookStatus.Cooked;
        }
        
        

        public void Cook(float dT)
        {
            _targetCookDuration = _recipe.timeToOvercook;
            CookDuration += dT;
            OnCook?.Invoke();
            UpdateTooltip();

            if (CookPercentage >= 1 && CookStatus != ECookStatus.Overcooked)
            {
                TransformToBurntMeal();
            }
        }
        private void TransformToBurntMeal()
        {
            CookStatus = ECookStatus.Overcooked;
            _onOvercooked?.Invoke();
        }
        public void StopCooking()
        {
            string text = _recipe.Name;
            if (CookStatus == ECookStatus.Overcooked) text += " (overcooked)";
        }
        private void UpdateTooltip()
        {
            string text;
            if (CookDuration == 0)
            {
                text = _recipe.Name;
            }
            else
            {
                text = "Cooking (" + Mathf.RoundToInt(CookPercentage * 100 + 100) + "%)";
            }
        }
        
        #endregion
    }

}