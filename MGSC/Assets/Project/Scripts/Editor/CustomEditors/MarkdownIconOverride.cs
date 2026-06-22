using UnityEditor;
using UnityEngine;

/// <summary>
/// Script generated with Claude.ai
/// </summary>
[InitializeOnLoad]
public class MarkdownIconOverride
{
    private static Texture2D _icon;

    static MarkdownIconOverride()
    {
        // Load your custom icon from the project
        _icon = Resources.Load<Texture2D>("Editor/Icons/Markdown_Icon");

        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
    }

    private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
    {
        if (_icon == null) return;

        string path = AssetDatabase.GUIDToAssetPath(guid);

        if (!path.EndsWith(".md", System.StringComparison.OrdinalIgnoreCase))
            return;
        if (!path.Replace(" ", "").Contains("todo", System.StringComparison.OrdinalIgnoreCase))
            return;

        // Draw the icon over the default one
        Rect iconRect = new Rect(selectionRect.x, selectionRect.y, selectionRect.width, selectionRect.height * 0.8f);

        // Erase default icon first
        EditorGUI.DrawRect(iconRect, new Color(0.22f, 0.22f, 0.22f)); // match Unity's dark bg

        GUI.DrawTexture(iconRect, _icon);
    }
}