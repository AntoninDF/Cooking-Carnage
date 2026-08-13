using NaughtyAttributes;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyPresetCollection", menuName = "Scriptable Objects/DifficultyPresetCollection")]
public class DifficultyPresetCollection : ScriptableObject
{
    public static DifficultyPresetCollection DefaultCollection => GetDefaultCollection();
    public static string defaultCollectionPath => "Difficulty/";
    public DifficultyPreset DefaultPreset;

    [Expandable]
    public List<DifficultyPreset> Presets;

    private static DifficultyPresetCollection defaultCollection;

    private static DifficultyPresetCollection GetDefaultCollection()
    {
        if (defaultCollection != null)
            return defaultCollection;

        string path = defaultCollectionPath;
        var collections = Resources.LoadAll<DifficultyPresetCollection>(path);
        if (collections.Length > 0)
            defaultCollection = collections[0];
        return defaultCollection;
    }

}
