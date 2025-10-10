using Controller;
using System.Collections;
using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// Adds every necessary ingredient behaviour
    /// </summary>
    public class Ingredient : BaseObject
    {
        #region Properties

        #endregion

        #region Fields

        [Header("References")]
        [Tooltip("Data model for this ingredient")]
        [SerializeField] private SO_Ingredient _model;
        [Tooltip("How long wil the ingredient take to go from a place to another when grabbed")]
        [SerializeField] private float _grabDuration = 0.2f;

        private Coroutine _coroutine;
        #endregion

        #region Methods

        public override void GetGrabbed(IContainer newContainer)
        {
            base.GetGrabbed(newContainer);
            GrabAnimation();

        }

        private void GrabAnimation()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
            SetTargetSlot();
            _coroutine = StartCoroutine(GoToTargetSlot(_grabDuration));
        }
        

        #endregion
    }

}