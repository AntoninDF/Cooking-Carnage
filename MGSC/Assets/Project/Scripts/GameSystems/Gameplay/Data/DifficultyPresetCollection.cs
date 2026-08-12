using JetBrains.Annotations;
using NaughtyAttributes;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyPresetCollection", menuName = "Scriptable Objects/DifficultyPresetCollection")]
public class DifficultyPresetCollection : ScriptableObject
{
    public static DifficultyPresetCollection DefaultCollection => GetDefaultCollection();
    public static string defaultCollectionGuid => "b82deb4297f50d24eb6b695d9f9a9b2c";
    public DifficultyPreset DefaultPreset;

    [Expandable]
    public List<DifficultyPreset> Presets;

    private static DifficultyPresetCollection defaultCollection;

    private static DifficultyPresetCollection GetDefaultCollection()
    {
        if (defaultCollection != null)
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(defaultCollection, out string guid, out long localid))
                if (guid == defaultCollectionGuid)
                    return defaultCollection;

        string path = AssetDatabase.GUIDToAssetPath(defaultCollectionGuid);
        defaultCollection = AssetDatabase.LoadAssetAtPath< DifficultyPresetCollection>(path);
        return defaultCollection;
    }

#if UNITY_EDITOR
    [Button("Make default collection")]
    private void WarningBeforeSettingDefaultAsset()
    {
        int val = EditorUtility.DisplayDialogComplex(
            "Warning",
            "Are you sure you want to set '" + name + "' as the default difficulty preset collection ?" +
            "\nThis will update it's guid and may break references.",
            "Yes, I'm sure",
            "No, cancel",
            "How do I get rid of this useless button ?");
        if (val == 0)
        {
            MakeDefaultCollection();
        }
    }
    private void MakeDefaultCollection()
    {
        if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(this, out string guid, out long localid))
            return;
        if (guid == defaultCollectionGuid)
            return;
        if (!AssetDatabase.IsMetaFileOpenForEdit(this))
            return;
        
        string assetPath = AssetDatabase.GetAssetPath(this);
        string metaPath = AssetDatabase.GetTextMetaFilePathFromAssetPath(assetPath);

        string result = "";
        foreach (var line in File.ReadLines(metaPath))
        {
            if (!line.Contains("guid"))
            {
                result += line + "\n";
                continue;
            }
            result += "guid: " + defaultCollectionGuid + "\n";
        }
        File.WriteAllText(metaPath, result);
        AssetDatabase.ImportAsset(assetPath);
    }
#endif
}
