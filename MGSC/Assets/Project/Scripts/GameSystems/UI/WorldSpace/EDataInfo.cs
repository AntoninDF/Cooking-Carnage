namespace GameSystem
{
    /// <summary>
    /// Enum that selects some data, used mainly for selecting text to display in the scene (for player feedbacks)
    /// </summary>
    public enum EDataInfo
    {
        // Warning : Do not reorder anything ! it will break inspector references.
        // Data easily accessible by any manager or script, and that is shared by all, goes here
        ShiftCount,
        CorrectOrdersThisShift,
        WrongOrdersThisShift,
        ClientsKilledThisShift,
        MoneyGainedThisShift,
        RentPayedThisShift,
        ShiftDuration,
        MoneyAmount,
        TimeElapsedSinceStartup,
        CookingSpeedUpgradeLevel,
        IngredientDropUpgradeLevel,
        OvenCountUpgradeLevel,
        CuttingSpeedUpgradeLevel,
        TimeLeft,
        MoneyGainedOnPreviousShift,

        // Data dependent on a specific object should be 50 or more
        ClientName = 50,
        IngredientName = 51,
        MealName = 52,
        MealIngredients = 53,
        MoneyGainedOnLastOrder = 54,
        MealPrice = 55,

        // Data passed by script
        ExternalData = 255
    }
}