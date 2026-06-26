using Unity.VisualScripting;
using UnityEngine;

public static partial class Globals
{
    public static class Gameplay
    {
        // CUTTING
        public static Param<bool> AllowCutting { get; set; } = true;
        public static Param<float> CuttingDuration { get; set; } = 3f;

        // CLIENTS
        public static Param<bool> AllowClientKilling { get; set; } = true;
        public static Param<bool> AllowClientSpawning { get; set; } = true;

        // COOKING
        public static Param<bool> AllowCooking { get; set; } = true;
        public static Param<float> CookingDuration { get; set; } = 3f;
        public static Param<float> OvercookingDuration { get; set; } = 5f;

        // SHIFT
        public static Param<int> ShiftNumber { get; set; } = 0;
        public static Param<float> ShiftDuration { get; set; } = 300f; // In seconds
        public static Param<float> ShiftFinalWarningDuration { get; set; } = 10f; // Must be less than ShiftDuration
        public static Param<AnimationCurve> ShiftRentPriceCurve { get; set; } = AnimationCurve.Linear(0,0,1,1);

        // PRICES
        public static Param<float> WrongMealPrice { get; set; } = 5f;
    }
}
