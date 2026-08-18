using UnityEngine;

public partial class Globals
{
    public class Gameplay
    {
        public static class Player
        {
            public static Param<float> LookSpeed { get; set; } = new(0.1f);
            public static Param<float> ScrollSensitivity { get; set; } = new(30f);

        }


        // CUTTING
        public static Param<float> CuttingDuration { get; set; } = new(3f);


        // COOKING
        public static Param<float> CookingDuration { get; set; } = new(3f);
        public static Param<float> OvercookingDuration { get; set; } = new(5f);

        // SHIFT
        public static Param<int> ShiftNumber { get; set; } = new(0);
        public static Param<float> ShiftDuration { get; set; } = new(300); // In seconds
        public static Param<float> ShiftFinalWarningDuration { get; set; } = new(10f); // Must be less than ShiftDuration
        public static Param<AnimationCurve> ShiftRentPriceCurve { get; set; } = new(AnimationCurve.Linear(0,0,1,1));

        // PRICES
        public static Param<float> WrongMealPrice { get; set; } = new(5f);
        public static Param<float> Threshold1Star { get; set; } = new(0.66f);
        public static Param<float> Threshold2Star { get; set; } = new(0.33f);
    }
}
