using Unity.VisualScripting;
using UnityEngine;

public static partial class Globals
{
    public static class Gameplay
    {
        public static Param<bool> AllowIngredientCutting { get; set; } = true;
        public static Param<bool> AllowClientKilling { get; set; } = true;
        public static Param<bool> AllowClientSpawning { get; set; } = true;
    }
}
