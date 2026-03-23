using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adds an Inventory bag button to the PlayScene HUD, next to the Settings button.
/// Wires the button to RecyclableInventoryManager.ToggleInventory().
///
/// Run via: Tools > Add Inventory Button to HUD
/// </summary>
public class AddInventoryButton
{
    private const string ICON_PATH = "Assets/Sprites/UI/icon_inventory.png";

    [MenuItem("Tools/Add Inventory Button to HUD")]
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
            Debug.Log("AddInventoryButton: Sprite import configured.");
        }
        else
        {
            Debug.LogError("AddInventoryButton: Icon not found at " + ICON_PATH);
            return;
        }

        // Find Canvas
        Canvas canvas = FindScreenSpaceCanvas();
        if (canvas == null)
        {
            Debug.LogError("AddInventoryButton: No Canvas found!");
            return;
        }

        // Remove old button if exists
        var oldBtn = canvas.transform.Find("InventoryButton");
        if (oldBtn != null)
        {
            Object.DestroyImmediate(oldBtn.gameObject);
        }

        // Load sprite
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_PATH);
        if (sprite == null)
        {
            Debug.LogError("AddInventoryButton: Failed to load sprite at " + ICON_PATH);
            return;
        }

        // Find SettingsButton to position relative to it
        var settingsBtn = canvas.transform.Find("SettingsButton");
        Vector2 position;
        if (settingsBtn != null)
        {
            var settingsRT = settingsBtn.GetComponent<RectTransform>();
            // Place to the left of the settings button (same Y, offset X by button width + spacing)
            position = settingsRT.anchoredPosition + new Vector2(-50f, 0f);
        }
        else
        {
            // Fallback: top-right, below gold/diamond, left of where settings would be
            position = new Vector2(-60f, -80f);
        }

        // Create button GO
        var btnGO = new GameObject("InventoryButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(canvas.transform, false);
        Undo.RegisterCreatedObjectUndo(btnGO, "Create InventoryButton");

        // Position: top-right area, next to settings
        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(40f, 40f);

        // Set icon
        var img = btnGO.GetComponent<Image>();
        img.sprite = sprite;
        img.color = Color.white;
        img.preserveAspect = true;

        // Style button
        var btn = btnGO.GetComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.9f, 0.6f);
        colors.pressedColor = new Color(0.8f, 0.7f, 0.4f);
        btn.colors = colors;

        // Wire button to RecyclableInventoryManager.ToggleInventory
        RecyclableInventoryManager invManager = null;
        foreach (var mgr in Resources.FindObjectsOfTypeAll<RecyclableInventoryManager>())
        {
            if (EditorUtility.IsPersistent(mgr)) continue;
            invManager = mgr;
            break;
        }

        if (invManager != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                btn.onClick, invManager.ToggleInventory);
            Debug.Log("AddInventoryButton: Wired to RecyclableInventoryManager.ToggleInventory()");
        }
        else
        {
            Debug.LogWarning("AddInventoryButton: RecyclableInventoryManager not found! Wire button manually.");
        }

        EditorUtility.SetDirty(btnGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("AddInventoryButton: Inventory button added to HUD!");
    }

    private static Canvas FindScreenSpaceCanvas()
    {
        foreach (var c in Resources.FindObjectsOfTypeAll<Canvas>())
        {
            if (EditorUtility.IsPersistent(c)) continue;
            if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                return c;
        }
        // Fallback: any canvas
        foreach (var c in Resources.FindObjectsOfTypeAll<Canvas>())
        {
            if (EditorUtility.IsPersistent(c)) continue;
            return c;
        }
        return null;
    }
}
