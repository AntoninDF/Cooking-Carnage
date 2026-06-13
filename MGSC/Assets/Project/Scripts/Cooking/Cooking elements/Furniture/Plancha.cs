using UnityEngine;

namespace Cooking
{

    /// <summary>
    /// Cooks objects put on top of it
    /// </summary>
    public class Plancha : SurfaceContainer
    {
        #region Properties
        public bool IsActive { get; set; } = true;
        public float Strength { get=>_strength; set=>_strength = value;} // Defines how fast the plancha cooks. 0 doesn't cook, 1 is normal speed, 2 is twice as fast...
        #endregion

        #region Fields
        [Header("Parameters")]
        [Tooltip("Speed modifier for the plancha")]
        [SerializeField] private float _strength = 1.0f;
        #endregion

        #region Methods
        // Update is called once per frame
        void Update()
        {
            if (!IsActive) return;

            CookItems();
        }

        private void CookItems()
        {
            foreach (IGrabbable item in HeldItems)
            {
                if (item is not ICookable) continue;

                ICookable itemToCook = item as ICookable;
                itemToCook.Cook(Time.deltaTime * Strength);
            }
        }

        #endregion
    }

}