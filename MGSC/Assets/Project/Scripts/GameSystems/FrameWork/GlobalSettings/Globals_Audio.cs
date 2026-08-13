using UnityEngine;

partial class Globals
{
    public static class Audio
    {
        public static Param<float> MasterVolume = new(1f);
        public static Param<float> MusicVolume = new(1f);
        public static Param<float> SFXVolume = new(1f);
        public static Param<float> AmbienceVolume = new(1f);
        public static Param<float> DialogVolume = new(1f);
    }

}
