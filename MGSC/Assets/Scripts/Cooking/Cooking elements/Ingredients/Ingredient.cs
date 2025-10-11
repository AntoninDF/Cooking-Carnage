using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// Implements the base behaviour for every ingredient
    /// </summary>
    public class Ingredient : Item
    {
        #region Properties

        #endregion

        #region Fields

        [Header("Ingredient references")]
        [Tooltip("Data model for this ingredient")]
        [SerializeField] protected SO_Ingredient _model;

        #endregion

        #region Methods

        #endregion

    }

}