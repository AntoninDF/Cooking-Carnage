using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// Defines the data and behaviour needed to, uh, be a meal
    /// </summary>
    public class Meal : Item
    {
        #region Properties
        public SO_Recipe Recipe { get => _recipe; set => _recipe = value; }
        #endregion

        #region Fields
        [Header("Meal references")]
        [Tooltip("Set what recipe this meal represents")]
        [SerializeField] private SO_Recipe _recipe;

        #endregion

        #region Methods

        #endregion
    }

}