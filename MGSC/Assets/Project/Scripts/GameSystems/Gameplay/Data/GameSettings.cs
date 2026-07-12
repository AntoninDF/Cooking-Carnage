using System;
using UnityEditor;
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
        Globals.Gameplay.Threshold1Star.Value = Instance.PriceSettings.Threshold1Star;
        Globals.Gameplay.Threshold2Star.Value = Instance.PriceSettings.Threshold2Star;

        Globals.Gameplay.CookingDuration.Value = Instance.CookingSettings.CookingDuration;
        Globals.Gameplay.OvercookingDuration.Value = Instance.CookingSettings.OverCookingDuration;

        Globals.Gameplay.Player.LookSpeed.Value = Instance.ControllerSettings.LookSensitivity;

        Globals.Rendering.Outline.Thickness.Value = Instance.GraphicsSettings.OutlineWidth;
        Globals.Rendering.Outline.Color.Value = Instance.GraphicsSettings.OutlineColor;

        /*
        Globals.Application.AllowWindowResizing.OnValueChange += value => PlayerSettings.resizableWindow = value;
        Globals.Application.AllowWindowResizing.Value = Instance.ApplicationSettings.AllowWindowResizing;
        */
    }

    void OnValidate() => Apply();

    public S_ShiftSettings ShiftSettings;
    [Serializable]
    public struct S_ShiftSettings
    {
        [Min(30), Tooltip("In seconds")]
        public int ShiftDuration;
        [Min(0), Tooltip("In seconds")]
        public int ShiftFinalWarningDuration;
    }

    public S_PriceSettings PriceSettings;
    [Serializable]
    public struct S_PriceSettings
    {
        public AnimationCurve ShiftRentPrice;
        [Min(0)]
        public float WrongMealPrice;

        public float Threshold1Star;
        public float Threshold2Star;
    }

    public S_CookingSettings CookingSettings;
    [Serializable]
    public struct S_CookingSettings
    {
        [Min(0)]
        public float CookingDuration;
        [Min(0)]
        public float OverCookingDuration;
    }

    public S_ControllerSettings ControllerSettings;
    [Serializable]
    public struct S_ControllerSettings
    {
        [Min(0.01f)]
        public float LookSensitivity;
    }
    
    public S_GraphicsSettings GraphicsSettings;
    [Serializable]
    public struct S_GraphicsSettings
    {
        [Min(0f)]
        public float OutlineWidth;
        public Color OutlineColor;
    }
    
    /*
    public S_ApplicationSettings ApplicationSettings;
    [Serializable]
    public struct S_ApplicationSettings
    {
        public bool AllowWindowResizing;
    }
    */
}
