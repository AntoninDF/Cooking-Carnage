using Cooking;
using UnityEngine;

namespace Orders
{
    /// <summary>
    /// Gives information on how, when, what... has been served
    /// </summary>
    public struct ServingInfo
    {
        public Meal Meal { get; private set; }
        public SO_Recipe Recipe => Meal.Recipe;
        public float Timestamp { get; private set; }

        public ServingInfo
            (
            Meal servedMeal
            )
        {
            Meal = servedMeal;
            Timestamp = Time.time;
        }

    }
}