using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script that applies Rustic UI sprites to all game UI panels:
/// SettingsPanel, DialoguePanel, StaminaHUD, WeatherHUD.
///
/// Configures 9-slice borders, Image types, and text styling for
/// a consistent Vietnamese farm aesthetic.
///
/// Run: Tools > Vietnamese Farmer > Apply Rustic UI to All Panels
/// </summary>
public class ApplyRusticUI : EditorWindow
{
    // ─── Sprite Paths (Rustic UI Pack) ───
    private const string SPRITE_ROOT   = "Assets/Rustic UI/UI-Singles/";
    private const string SP_PANEL_LG   = SPRITE_ROOT + "UI - 1.png";   // Large panel bg
    private const string SP_PANEL_MD   = SPRITE_ROOT + "UI - 50.png";  // Medium panel bg
    private const string SP_PANEL_DARK = SPRITE_ROOT + "UI - 55.png";  // Dark panel
    private const string SP_BUTTON     = SPRITE_ROOT + "UI - 15.png";  // Rounded button
    private const string SP_BUTTON_DK  = SPRITE_ROOT + "UI - 10.png";  // Dark button
    private const string SP_SLIDER_BG  = SPRITE_ROOT + "UI - 40.png";  // Slider track (light)
    private const string SP_SLIDER_FILL= SPRITE_ROOT + "UI - 30.png";  // Slider fill (orange)
    private const string SP_KNOB       = SPRITE_ROOT + "UI - 20.png";  // Circle knob
    private const string SP_TITLE_BAR  = SPRITE_ROOT + "UI - 60.png";  // Title bar panel
    private const string SP_ICON_CHECK = SPRITE_ROOT + "UI - 80.png";  // Checkbox icon
    private const string SP_ICON_X     = SPRITE_ROOT + "UI - 84.png";  // Close X icon

    // ─── Text Colors ───
    private static readonly Color TEXT_TITLE  = new Color(1f, 0.92f, 0.70f, 1f);    // warm cream
    private static readonly Color TEXT_BODY   = new Color(1f, 0.96f, 0.85f, 1f);    // light cream
    private static readonly Color TEXT_DIM    = new Color(0.85f, 0.78f, 0.60f, 1f); // muted gold
    private static readonly Color TEXT_BTN    = new Color(1f, 0.95f, 0.80f, 1f);    // button text

    [MenuItem("Tools/Vietnamese Farmer/Apply Rustic UI to All Panels")]
    public static void Apply()
    {
        Debug.Log("=== ApplyRusticUI: Starting ===");

        // Ensure all sprites have correct import settings
        ConfigureSpriteImports();

        // Apply to each panel
        ApplySettingsPanel();
        ApplyDialoguePanel();
        ApplyStaminaHUD();
        ApplyWeatherHUD();

        // Mark scene dirty so it can be saved
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("=== ApplyRusticUI: Complete! ===");
    }

    // ════════════════════════════════════════════
    // SPRITE IMPORT CONFIG
    // ════════════════════════════════════════════

    private static void ConfigureSpriteImports()
    {
        // Large panel: 9-slice with generous borders
        SetSliceBorders(SP_PANEL_LG, new Vector4(6, 6, 6, 6));
        SetSliceBorders(SP_PANEL_MD, new Vector4(6, 6, 6, 6));
        SetSliceBorders(SP_PANEL_DARK, new Vector4(6, 6, 6, 6));
        SetSliceBorders(SP_BUTTON, new Vector4(6, 6, 6, 6));
        SetSliceBorders(SP_BUTTON_DK, new Vector4(6, 6, 6, 6));
        SetSliceBorders(SP_SLIDER_BG, new Vector4(5, 3, 5, 3));
        SetSliceBorders(SP_SLIDER_FILL, new Vector4(4, 2, 4, 2));
        SetSliceBorders(SP_TITLE_BAR, new Vector4(6, 6, 6, 6));

        AssetDatabase.Refresh();
        Debug.Log("ApplyRusticUI: Sprite import settings configured.");
    }

    private static void SetSliceBorders(string path, Vector4 border)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"ApplyRusticUI: Sprite not found at {path}");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        // Set 9-slice borders
        importer.spriteBorder = border;

        // Pixel-perfect settings
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spritePixelsPerUnit = 16;
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);

        importer.SaveAndReimport();
    }

    // ════════════════════════════════════════════
    // SETTINGS PANEL
    // ════════════════════════════════════════════

    private static void ApplySettingsPanel()
    {
        GameObject panel = GameObject.Find("SettingsPanel");
        if (panel == null) { Debug.LogWarning("ApplyRusticUI: SettingsPanel not found"); return; }

        // Main panel background
        SetSlicedImage(panel, SP_PANEL_LG, Color.white);

        // Title text
        StyleText(FindChild(panel, "Title"), TEXT_TITLE, 18, FontStyles.Bold);

        // BGM row
        StyleText(FindChild(panel, "BGMLabel"), TEXT_BODY, 13);
        StyleText(FindChild(panel, "BGMValue"), TEXT_DIM, 12);
        ApplySliderSkin(FindChild(panel, "BGMSlider"));

        // SFX row
        StyleText(FindChild(panel, "SFXLabel"), TEXT_BODY, 13);
        StyleText(FindChild(panel, "SFXValue"), TEXT_DIM, 12);
        ApplySliderSkin(FindChild(panel, "SFXSlider"));

        // Fullscreen toggle
        ApplyToggleSkin(FindChild(panel, "FullscreenToggle"));
        StyleText(FindChild(panel, "FullscreenLabel"), TEXT_BODY, 13);

        // Close button
        ApplyButtonSkin(FindChild(panel, "CloseButton"));
        StyleText(FindChildRecursive(panel, "CloseLabel"), TEXT_BTN, 13, FontStyles.Bold);

        Debug.Log("ApplyRusticUI: SettingsPanel styled.");
    }

    // ════════════════════════════════════════════
    // DIALOGUE PANEL
    // ════════════════════════════════════════════

    private static void ApplyDialoguePanel()
    {
        GameObject panel = GameObject.Find("DialoguePanel");
        if (panel == null) { Debug.LogWarning("ApplyRusticUI: DialoguePanel not found"); return; }

        // Main panel background
        SetSlicedImage(panel, SP_PANEL_LG, Color.white);

        // Speaker name
        StyleText(FindChild(panel, "SpeakerName"), TEXT_TITLE, 15, FontStyles.Bold);

        // Dialogue body
        StyleText(FindChild(panel, "DialogueText"), TEXT_BODY, 14);

        // Continue prompt
        StyleText(FindChild(panel, "ContinuePrompt"), TEXT_DIM, 11, FontStyles.Italic);

        Debug.Log("ApplyRusticUI: DialoguePanel styled.");
    }

    // ════════════════════════════════════════════
    // STAMINA HUD
    // ════════════════════════════════════════════

    private static void ApplyStaminaHUD()
    {
        GameObject hud = GameObject.Find("StaminaHUD");
        if (hud == null) { Debug.LogWarning("ApplyRusticUI: StaminaHUD not found"); return; }

        // Background panel
        SetSlicedImage(hud, SP_PANEL_MD, Color.white);

        // Slider skin
        GameObject slider = FindChild(hud, "StaminaSlider");
        if (slider != null)
        {
            // Slider background image (if exists)
            Image sliderImg = slider.GetComponent<Image>();
            if (sliderImg != null)
            {
                Sprite bgSprite = LoadSprite(SP_SLIDER_BG);
                if (bgSprite != null)
                {
                    sliderImg.sprite = bgSprite;
                    sliderImg.type = Image.Type.Sliced;
                    sliderImg.color = new Color(0.4f, 0.3f, 0.2f, 0.8f);
                }
            }
        }

        // Fill color stays dynamic (set by StaminaHUD script at runtime)
        // But we style the fill image's base sprite
        GameObject fillArea = FindChildRecursive(hud, "FillArea");
        if (fillArea != null)
        {
            Image fillAreaImg = fillArea.GetComponent<Image>();
            if (fillAreaImg != null) fillAreaImg.color = new Color(0, 0, 0, 0); // transparent
        }

        GameObject fill = FindChildRecursive(hud, "Fill");
        if (fill != null)
        {
            Image fillImg = fill.GetComponent<Image>();
            if (fillImg != null)
            {
                Sprite fillSprite = LoadSprite(SP_SLIDER_FILL);
                if (fillSprite != null)
                {
                    fillImg.sprite = fillSprite;
                    fillImg.type = Image.Type.Sliced;
                }
            }
        }

        // Text
        StyleText(FindChild(hud, "StaminaLabel"), TEXT_BODY, 11);

        Debug.Log("ApplyRusticUI: StaminaHUD styled.");
    }

    // ════════════════════════════════════════════
    // WEATHER HUD
    // ════════════════════════════════════════════

    private static void ApplyWeatherHUD()
    {
        GameObject hud = GameObject.Find("WeatherHUD");
        if (hud == null) { Debug.LogWarning("ApplyRusticUI: WeatherHUD not found"); return; }

        // Background panel
        SetSlicedImage(hud, SP_PANEL_MD, Color.white);

        // Weather text color is set dynamically by WeatherHUD script
        // Just ensure good base styling
        StyleText(FindChildRecursive(hud, "WeatherText"), TEXT_BODY, 13);

        Debug.Log("ApplyRusticUI: WeatherHUD styled.");
    }

    // ════════════════════════════════════════════
    // UI COMPONENT HELPERS
    // ════════════════════════════════════════════

    private static void ApplySliderSkin(GameObject sliderGO)
    {
        if (sliderGO == null) return;

        // The Slider GO itself gets a background track
        Image bgImg = sliderGO.GetComponent<Image>();
        if (bgImg != null)
        {
            Sprite trackSprite = LoadSprite(SP_SLIDER_BG);
            if (trackSprite != null)
            {
                bgImg.sprite = trackSprite;
                bgImg.type = Image.Type.Sliced;
                bgImg.color = Color.white;
            }
        }

        // Find and style Fill rect
        Transform fillArea = sliderGO.transform.Find("Fill Area");
        if (fillArea == null) fillArea = sliderGO.transform.Find("FillArea");
        if (fillArea != null)
        {
            Transform fill = fillArea.Find("Fill");
            if (fill != null)
            {
                Image fillImg = fill.GetComponent<Image>();
                if (fillImg != null)
                {
                    Sprite fillSprite = LoadSprite(SP_SLIDER_FILL);
                    if (fillSprite != null)
                    {
                        fillImg.sprite = fillSprite;
                        fillImg.type = Image.Type.Sliced;
                        fillImg.color = new Color(0.95f, 0.75f, 0.25f, 1f); // warm gold
                    }
                }
            }
        }

        // Find and style Handle
        Transform handleArea = sliderGO.transform.Find("Handle Slide Area");
        if (handleArea != null)
        {
            Transform handle = handleArea.Find("Handle");
            if (handle != null)
            {
                Image handleImg = handle.GetComponent<Image>();
                if (handleImg != null)
                {
                    Sprite knobSprite = LoadSprite(SP_KNOB);
                    if (knobSprite != null)
                    {
                        handleImg.sprite = knobSprite;
                        handleImg.color = Color.white;
                    }
                }
            }
        }
    }

    private static void ApplyButtonSkin(GameObject btnGO)
    {
        if (btnGO == null) return;

        Image img = btnGO.GetComponent<Image>();
        if (img == null) return;

        Sprite btnSprite = LoadSprite(SP_BUTTON);
        if (btnSprite != null)
        {
            img.sprite = btnSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }

        // Button component color tinting
        Button btn = btnGO.GetComponent<Button>();
        if (btn != null)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor      = Color.white;
            colors.highlightedColor = new Color(1f, 0.9f, 0.7f, 1f);
            colors.pressedColor     = new Color(0.8f, 0.7f, 0.5f, 1f);
            colors.selectedColor    = new Color(1f, 0.95f, 0.8f, 1f);
            btn.colors = colors;
        }
    }

    private static void ApplyToggleSkin(GameObject toggleGO)
    {
        if (toggleGO == null) return;

        // Toggle background
        Image img = toggleGO.GetComponent<Image>();
        if (img != null)
        {
            Sprite bgSprite = LoadSprite(SP_BUTTON_DK);
            if (bgSprite != null)
            {
                img.sprite = bgSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
        }

        // Toggle checkmark
        Toggle toggle = toggleGO.GetComponent<Toggle>();
        if (toggle != null && toggle.graphic != null)
        {
            Image checkImg = toggle.graphic as Image;
            if (checkImg != null)
            {
                Sprite checkSprite = LoadSprite(SP_ICON_CHECK);
                if (checkSprite != null)
                {
                    checkImg.sprite = checkSprite;
                    checkImg.color = new Color(0.95f, 0.85f, 0.45f, 1f); // gold check
                }
            }
        }
    }

    // ════════════════════════════════════════════
    // UTILITY
    // ════════════════════════════════════════════

    private static void SetSlicedImage(GameObject go, string spritePath, Color tint)
    {
        if (go == null) return;

        Image img = go.GetComponent<Image>();
        if (img == null)
        {
            img = go.AddComponent<Image>();
        }

        Sprite sprite = LoadSprite(spritePath);
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = tint;
            img.pixelsPerUnitMultiplier = 1f;
        }
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogWarning($"ApplyRusticUI: Could not load sprite at {path}");
        }
        return sprite;
    }

    private static void StyleText(GameObject go, Color color, float fontSize, FontStyles style = FontStyles.Normal)
    {
        if (go == null) return;

        TMP_Text tmp = go.GetComponent<TMP_Text>();
        if (tmp == null) return;

        tmp.color = color;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;

        // Add subtle shadow for readability on wooden bg
        tmp.enableVertexGradient = false;
        tmp.outlineWidth = 0.15f;
        tmp.outlineColor = new Color32(40, 25, 10, 180);
    }

    private static GameObject FindChild(GameObject parent, string name)
    {
        if (parent == null) return null;
        Transform t = parent.transform.Find(name);
        return t != null ? t.gameObject : null;
    }

    private static GameObject FindChildRecursive(GameObject parent, string name)
    {
        if (parent == null) return null;

        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name) return child.gameObject;
        }
        return null;
    }
}
