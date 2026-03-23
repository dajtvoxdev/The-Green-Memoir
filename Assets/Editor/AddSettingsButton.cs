using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adds a Settings gear button to the PlayScene HUD.
/// Configures the icon sprite import and wires the button to SettingsPanel.TogglePanel().
///
/// Run via: Tools > Add Settings Button to HUD
/// </summary>
public class AddSettingsButton
{
    private const string ICON_PATH = "Assets/Sprites/UI/icon_settings.png";

    [MenuItem("Tools/Add Settings Button to HUD")]
    public static void AddButton()
    {
        // Configure sprite import
        var importer = AssetImporter.GetAtPath(ICON_PATH) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            Debug.Log("AddSettingsButton: Sprite import configured.");
        }
        else
        {
            Debug.LogError("AddSettingsButton: Icon not found at " + ICON_PATH);
            return;
        }

        // Find Canvas
        Canvas canvas = null;
        foreach (var c in Resources.FindObjectsOfTypeAll<Canvas>())
        {
            if (EditorUtility.IsPersistent(c)) continue;
            if (c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas = c;
                break;
            }
        }
        if (canvas == null)
        {
            foreach (var c in Resources.FindObjectsOfTypeAll<Canvas>())
            {
                if (EditorUtility.IsPersistent(c)) continue;
                canvas = c;
                break;
            }
        }
        if (canvas == null)
        {
            Debug.LogError("AddSettingsButton: No Canvas found!");
            return;
        }

        // Remove old button if exists
        var oldBtn = canvas.transform.Find("SettingsButton");
        if (oldBtn != null)
        {
            Object.DestroyImmediate(oldBtn.gameObject);
        }

        // Load sprite
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_PATH);
        if (sprite == null)
        {
            Debug.LogError("AddSettingsButton: Failed to load sprite at " + ICON_PATH);
            return;
        }

        // Create button GO
        var btnGO = new GameObject("SettingsButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(canvas.transform, false);

        // Position: top-right area, below the gold/diamond HUD
        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-10f, -80f); // Below gold/diamond
        rt.sizeDelta = new Vector2(40f, 40f);

        // Set icon
        var img = btnGO.GetComponent<Image>();
        img.sprite = sprite;
        img.color = Color.white;
        img.preserveAspect = true;

        // Style button (no color tint on normal, slight highlight)
        var btn = btnGO.GetComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.9f, 0.6f);
        colors.pressedColor = new Color(0.8f, 0.7f, 0.4f);
        btn.colors = colors;

        // Wire button to SettingsPanel.TogglePanel
        SettingsPanel settingsPanel = null;
        foreach (var sp in Resources.FindObjectsOfTypeAll<SettingsPanel>())
        {
            if (EditorUtility.IsPersistent(sp)) continue;
            settingsPanel = sp;
            break;
        }

        if (settingsPanel != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                btn.onClick, settingsPanel.TogglePanel);
            Debug.Log("AddSettingsButton: Wired to SettingsPanel.TogglePanel()");
        }
        else
        {
            Debug.LogWarning("AddSettingsButton: SettingsPanel not found! Wire button manually.");
        }

        EditorUtility.SetDirty(btnGO);
        Debug.Log("AddSettingsButton: Settings button added to HUD!");
    }
}
