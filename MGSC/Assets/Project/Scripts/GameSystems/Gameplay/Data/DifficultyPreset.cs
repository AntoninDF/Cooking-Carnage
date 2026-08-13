using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Difficulty preset", menuName = "Scriptable Objects/Difficulty preset")]
[HelpBox("How to fill in the settings :\n\n" +
    "- The name for each setting should be exactly the one displayed in the settings menu.\n" +
    "- The values for percentages should be normalized to the 0-1 range (2 = 200%).\n" +
    "- Everything is a multiplier, not a setter.\n" +
    "- The name of the asset will be used for the preset's name.")]
public class DifficultyPreset : ScriptableObject
{
    public List<DifficultySetting> DifficultySettingList;


    [Serializable]
    public struct DifficultySetting
    {
        public string Name;
        public float Value;
    }
}
