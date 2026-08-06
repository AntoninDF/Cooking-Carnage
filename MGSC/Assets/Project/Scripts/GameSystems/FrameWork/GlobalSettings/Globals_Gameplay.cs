using Unity.VisualScripting;
using UnityEngine;

public partial class Globals
{
    public class Gameplay
    {
        // TIMERS
        public static Param<bool> AllowTimerUpdate { get; set; } = new(true);


        public static class Player
        {
            public static Param<bool> AllowControllerMovement { get; set; } = new(true);
            public static Param<bool> AllowPlayerInteractions { get; set; } = new(true);
            public static Param<bool> AllowPlayerGrab { get; set; } = new(true);
            public static Param<float> LookSpeed { get; set; } = new(0.1f);
            public static Param<float> ScrollSensitivity { get; set; } = new(0.2f);

        }

        // PLATES
        public static Param<bool> AllowPuttingIngredientsInPlates { get; set; } = new(true);

        // CUTTING
        public static Param<bool> AllowCutting { get; set; } = new(true);
        public static Param<float> CuttingDuration { get; set; } = new(3f);

        // CLIENTS
        public static Param<bool> AllowClientKilling { get; set; } = new(true);
        public static Param<bool> AllowClientServing { get; set; } = new(true);
        public static Param<bool> AllowClientSpawning { get; set; } = new(true);

        // COOKING
        public static Param<bool> AllowCooking { get; set; } = new(true);
        public static Param<bool> AllowPuttingPlatesInOvens { get; set; } = new(true);
        public static Param<float> CookingDuration { get; set; } = new(3f);
        public static Param<float> OvercookingDuration { get; set; } = new(5f);

        // SHIFT
        public static Param<bool> AllowShiftStart { get; set; } = new(true);
        public static Param<int> ShiftNumber { get; set; } = new(0);
        public static Param<float> ShiftDuration { get; set; } = new(3000); // In seconds
        public static Param<float> ShiftFinalWarningDuration { get; set; } = new(10f); // Must be less than ShiftDuration
        public static Param<AnimationCurve> ShiftRentPriceCurve { get; set; } = new(AnimationCurve.Linear(0,0,1,1));

        // PRICES
        public static Param<float> WrongMealPrice { get; set; } = new(5f);
        public static Param<float> Threshold1Star { get; set; } = new(0.66f);
        public static Param<float> Threshold2Star { get; set; } = new(0.33f);
        
        // UPGRADES
        public static Param<bool> AllowUpgrades { get; set; } = new(true);
        
        //BLOOD
        public static Param<bool> AllowBlending { get; set; } = new(true); //set to false after debugging
    }
}
