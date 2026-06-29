using NaughtyAttributes;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/GameSettings")]
public class GameSettings : ScriptableObject
{
    static GameSettings _instance;
    public static GameSettings Instance
        { get {
            _instance = _instance ?? Resources.Load<GameSettings>("GameSettings");
            return _instance; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Apply()
    {
        Globals.Gameplay.ShiftDuration.Value = Instance.ShiftSettings.ShiftDuration;
        Globals.Gameplay.ShiftFinalWarningDuration.Value = Instance.ShiftSettings.ShiftFinalWarningDuration;

        Globals.Gameplay.ShiftRentPriceCurve.Value = Instance.PriceSettings.ShiftRentPrice;
        Globals.Gameplay.WrongMealPrice.Value = Instance.PriceSettings.WrongMealPrice;
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

}
