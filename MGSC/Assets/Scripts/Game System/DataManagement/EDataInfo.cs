using UnityEngine;

namespace GameSystem
{

    public enum EDataInfo
    {
        ShiftCount,
        CorrectOrdersThisShift,
        WrongOrdersThisShift,
        ClientsKilledThisShift,
        MoneyGainedThisShift,
        RentPayedThisShift,
        ShiftDuration,
        MoneyAmount,
        TimeElapsedSinceStartup,


        // Data dependent on a specific object should be 50 or more
        ClientName = 50,
        IngredientName = 51,
        MealName = 52,

    }
}