using NaughtyAttributes;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/GameSettings")]
public class GameSettings : ScriptableObject
{
    static GameSettings _instance;
    public static GameSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameSettings>("GameSettings");
            }
            return _instance;
        }
    }

    public S_ShiftSettings ShiftSettings;
    [Serializable]
    public struct S_ShiftSettings
    {
        [Min(30), Tooltip("In seconds")]
        public float ShiftDuration;
        [MaxValue(30), Tooltip("In seconds")]
        public float ShiftFinalWarningDuration;
    }

    public S_PriceSettings PriceSettings;
    [Serializable]
    public struct S_PriceSettings
    {
        public AnimationCurve ShiftRentPrice;
        [Min(0)]
        public float WrongMealPrice;
    }

    public void Apply()
    {
        Globals.Gameplay.ShiftDuration = ShiftSettings.ShiftDuration;
        Globals.Gameplay.ShiftFinalWarningDuration = ShiftSettings.ShiftFinalWarningDuration;

        Globals.Gameplay.ShiftRentPriceCurve = PriceSettings.ShiftRentPrice;
        Globals.Gameplay.WrongMealPrice = PriceSettings.WrongMealPrice;
    }
}
