partial class Globals
{
    public static class Authorizations
    {
        // TIMERS
        public static Param<bool> AllowTimerUpdate { get; set; } = new(true);

        public static class Player
        {
            public static Param<bool> AllowControllerMovement { get; set; } = new(true);
            public static Param<bool> AllowPlayerInteractions { get; set; } = new(true);
            public static Param<bool> AllowPlayerGrab { get; set; } = new(true);
        }

        // GLOOBY TRASH
        public static Param<bool> AllowTrashingStuff { get; set; } = new(true);
        // PLATES
        public static Param<bool> AllowPuttingIngredientsInPlates { get; set; } = new(true);

        // CUTTING
        public static Param<bool> AllowCutting { get; set; } = new(true);

        // CLIENTS
        public static Param<bool> AllowClientKilling { get; set; } = new(true);
        public static Param<bool> AllowClientServing { get; set; } = new(true);
        public static Param<bool> AllowClientSpawning { get; set; } = new(true);


        // COOKING
        public static Param<bool> AllowCooking { get; set; } = new(true);
        public static Param<bool> AllowPuttingPlatesInOvens { get; set; } = new(true);


        // SHIFT
        public static Param<bool> AllowShiftStart { get; set; } = new(true);


        // UPGRADES
        public static Param<bool> AllowUpgrades { get; set; } = new(true);
    }
}

