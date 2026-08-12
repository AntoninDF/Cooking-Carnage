using UnityEngine;

partial class Globals
{
    public static class BalancingWeights
    {

        // CLIENTS
        public static Param<float> RecipePriceMultiplier { get; set; } = new(1f);
        public static Param<float> ClientPatienceMultiplier { get; set; } = new(1f);

        // COOKING
        public static Param<float> PreparationFailTimeMultiplier { get; set; } = new(1f);

        // SHIFT
        public static Param<float> ShiftDurationMultiplier { get; set; } = new(1f);
        public static Param<float> RentPriceMultiplier { get; set; } = new(1f);

        // UPGRADES
        public static Param<float> UpgradePriceMultiplier { get; set; } = new(1f);
    }
}
