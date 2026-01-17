using UnityEngine;

namespace GameSystem
{

    public enum EDataInfo
    {
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