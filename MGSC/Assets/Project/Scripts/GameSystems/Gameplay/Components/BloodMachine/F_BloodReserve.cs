using System;
using Features;

namespace Project.Scripts.BloodMachine
{
    /// <summary>
    /// Wrapper for the available amount of blood of a given type
    /// </summary>
    public class F_BloodReserve : Feature
    {
        #region Properties
        public float BloodPerDrink => _bloodPerDrink;
        /// <summary>
        /// Modifying the value of <see cref="BloodQuantity"/> will invoke <see cref="OnBloodQuantityChange"/>.
        /// This allows the reserve's visual appearance to be updated automatically when the blood quantity changes.
        /// </summary>
        public float BloodQuantity
        {
            get => _quantity;
            set
            {
                if (value == _quantity) return;
                _quantity = value;
                OnBloodQuantityChange?.Invoke(value);
            }
        }
        public Action<float> OnBloodQuantityChange { get; set; }
        #endregion
       
        #region Fields
        private float _bloodPerDrink;
        private float _quantity = 0f;
        #endregion

        public F_BloodReserve(IFeatureCollection owner, float bloodPerDrink) : base(owner)
        {
            _bloodPerDrink     = bloodPerDrink;
        }
        public bool CanServeDrink() => _quantity >= _bloodPerDrink;
    }
}