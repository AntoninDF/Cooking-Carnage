using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tool that disables material import (Materials → "Import Materials")
/// on the selected imported model assets (e.g. .fbx), so Unity stops generating
/// materials from the model file and falls back to the default material instead.
/// </summary>
public static class DisableMaterialImportForSelectedModels
{
    [MenuItem("Assets/Optimizations/Disable material import for selected models", true)]
    private static bool ValidateDisableMaterialImport()
    {
        foreach (var obj in Selection.gameObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(assetPath) && AssetImporter.GetAtPath(assetPath) is ModelImporter)
                return true;
        }
        return false;
    }

    [MenuItem("Assets/Optimizations/Disable material import for selected models")]
    private static void DisableMaterialImport()
    {
        GameObject[] selectedModels = Selection.gameObjects;
        int modifiedModelCount = 0;

        // Batches reimports at StopAssetEditing() instead of one-by-one per SaveAndReimport call.
        AssetDatabase.StartAssetEditing();
        try
        {
            for (int i = 0; i < selectedModels.Length; i++)
            {
                GameObject selected = selectedModels[i];
                string assetPath = AssetDatabase.GetAssetPath(selected);

                if (string.IsNullOrEmpty(assetPath))
                    continue;

                if (!(AssetImporter.GetAtPath(assetPath) is ModelImporter modelImporter))
                    continue;

                EditorUtility.DisplayProgressBar(
                    "Disabling material import",
                    $"Processing {assetPath}",
                    (float)i / selectedModels.Length);

                if (modelImporter.materialImportMode != ModelImporterMaterialImportMode.None)
                {
                    modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
                    modelImporter.SaveAndReimport();
                    modifiedModelCount++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();

        Debug.Log($"Disable material import: modified {modifiedModelCount} model asset(s).");
    }
}
