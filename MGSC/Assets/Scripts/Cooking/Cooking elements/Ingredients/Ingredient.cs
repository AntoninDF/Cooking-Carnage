using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// Implements the base behaviour for every ingredient
    /// </summary>
    public class Ingredient : Item, ITrashable
    {
        #region Properties
        public SO_Ingredient Model => _model;
        #endregion

        #region Fields

        [Header("Ingredient references")]
        [Tooltip("Data model for this ingredient")]
        [SerializeField] protected SO_Ingredient _model;

        #endregion

        #region Methods

        protected override void Start()
        {
            base.Start();
            if (_model == null)
                throw new MissingReferenceException("Please assign an ingredient model in the inspector : " + name);
        }

        #endregion

    }

}