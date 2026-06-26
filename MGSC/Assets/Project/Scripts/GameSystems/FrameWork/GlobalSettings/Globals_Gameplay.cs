using Unity.VisualScripting;
using UnityEngine;

public static partial class Globals
{
    public static class Gameplay
    {
        public static Param<bool> AllowIngredientCutting { get; set; } = true;
        public static Param<bool> AllowClientKilling { get; set; } = true;
        public static Param<bool> AllowClientSpawning { get; set; } = true;
        public static Param<float> CookingDuration { get; set; } = 3f;
        public static Param<float> OvercookingDuration { get; set; } = 5f;
    }
}
