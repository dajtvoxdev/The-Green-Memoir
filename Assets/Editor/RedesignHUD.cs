using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Complete HUD builder for Moonlit Garden farming game.
/// Creates EconomyHUD (gold/diamond) and TimeHUD (day/time/period) from scratch
/// if they don't exist, or redesigns them if they do.
/// Uses Rustic UI wood panel backgrounds for polished farming game look.
///
/// Run via menu: Tools > Moonlit Garden > Redesign HUD
/// </summary>
public class RedesignHUD
{
    private const string PANEL_BG_PATH = "Assets/Rustic UI/UI-Singles/UI - 1.png";
    private const string PANEL_BG2_PATH = "Assets/Rustic UI/UI-Singles/UI - 2.png";
    private const string COIN_PATH = "Assets/Tiny RPG Forest/Artwork/sprites/misc/coin/coin-1.png";
    private const string GEM_PATH = "Assets/Tiny RPG Forest/Artwork/sprites/misc/gem/gem-1.png";
    private const string FONT_PATH = "Assets/UI/Fonts/Cherry_Bomb_One/CherryBombOne-Regular SDF.asset";
    private const string FALLBACK_FONT_PATH = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    private static readonly Color WARM_WHITE = new Color(1f, 0.96f, 0.88f, 1f);
    private static readonly Color PERIOD_MORNING = new Color(1f, 0.9f, 0.4f, 1f);

    [MenuItem("Tools/Moonlit Garden/Redesign HUD")]
    public static void Redesign()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("RedesignHUD: No Canvas found!");
            return;
        }

        // Try Cherry Bomb font, fallback to LiberationSans if atlas is broken
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);
        if (font == null || font.atlasTexture == null)
        {
            Debug.LogWarning("RedesignHUD: Cherry Bomb font missing or atlas broken, using fallback font");
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FALLBACK_FONT_PATH);
        }
        if (font == null)
        {
            // Last resort: use TMP default
            font = TMP_Settings.defaultFontAsset;
        }
        if (font == null)
        {
            Debug.LogError("RedesignHUD: No usable font found!");
            return;
        }
        Debug.Log($"RedesignHUD: Using font '{font.name}'");

        // Hide old UI elements that may overlap
        HideOldElements(canvas);

        // Fix ALL TMP components using the broken Cherry Bomb font
        FixBrokenFonts(canvas, font);

        // Build or rebuild HUDs
        BuildEconomyHUD(canvas, font);
        BuildTimeHUD(canvas, font);

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("RedesignHUD: HUD redesign complete!");
    }

    /// <summary>
    /// Replaces broken CherryBombOne SDF font on ALL TMP components in the Canvas
    /// with the working fallback font.
    /// </summary>
    static void FixBrokenFonts(Canvas canvas, TMP_FontAsset goodFont)
    {
        int fixedCount = 0;
        foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (tmp.font == null || tmp.font.atlasTexture == null)
            {
                tmp.font = goodFont;
                if (goodFont != null && goodFont.material != null)
                {
                    tmp.fontSharedMaterial = goodFont.material;
                }
                tmp.SetAllDirty();
                fixedCount++;
            }
        }
        if (fixedCount > 0)
        {
            Debug.Log($"RedesignHUD: Fixed {fixedCount} TMP components with broken font atlas");
        }
    }

    static void HideOldElements(Canvas canvas)
    {
        string[] hideNames = { "HUDTopBarBG", "UI Name Wizard", "UI Information In Game" };
        foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
        {
            foreach (string n in hideNames)
            {
                if (t.name == n) t.gameObject.SetActive(false);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  ECONOMY HUD: Top-right panel showing Gold | Diamond
    // ═══════════════════════════════════════════════════════════
    static void BuildEconomyHUD(Canvas canvas, TMP_FontAsset font)
    {
        // Find or create root
        Transform eHud = FindChild(canvas.transform, "EconomyHUD");
        if (eHud == null)
        {
            GameObject go = new GameObject("EconomyHUD", typeof(RectTransform), typeof(Image));
            go.AddComponent<EconomyHUD>();
            go.transform.SetParent(canvas.transform, false);
            eHud = go.transform;
            Debug.Log("RedesignHUD: Created EconomyHUD GameObject");
        }

        // Ensure parented directly to Canvas (not nested under inactive parents)
        if (eHud.parent != canvas.transform)
        {
            eHud.SetParent(canvas.transform, false);
            Debug.Log("RedesignHUD: Re-parented EconomyHUD to Canvas");
        }

        eHud.gameObject.SetActive(true);
        eHud.gameObject.layer = 5; // UI layer

        // Remove old LayoutGroups
        foreach (var lg in eHud.GetComponents<LayoutGroup>()) Object.DestroyImmediate(lg);
        foreach (var csf in eHud.GetComponents<ContentSizeFitter>()) Object.DestroyImmediate(csf);

        // Position: top-right
        var rt = eHud.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-16, -12);
        rt.sizeDelta = new Vector2(260, 48);
        rt.localScale = Vector3.one;
        FixZ(rt);

        // Background panel
        SetupPanelBG(eHud, PANEL_BG_PATH);

        // --- Gold (left half): Icon + Text ---
        Transform goldT = FindOrCreateChild(eHud, "Gold", true);
        SetupCurrencySlot(goldT, font, COIN_PATH, "GoldIcon",
            anchorMin: new Vector2(0, 0), anchorMax: new Vector2(0.48f, 1),
            padding: new Vector4(6, 4, 4, 4));

        // --- Diamond (right half): Icon + Text ---
        Transform diaT = FindOrCreateChild(eHud, "Diamond", true);
        SetupCurrencySlot(diaT, font, GEM_PATH, "DiamondIcon",
            anchorMin: new Vector2(0.52f, 0), anchorMax: new Vector2(1, 1),
            padding: new Vector4(4, 4, 6, 4));

        // Separator line between gold and diamond
        SetupSeparator(eHud, "Separator", 0.5f);

        // Wire up EconomyHUD component references
        var economyHUD = eHud.GetComponent<EconomyHUD>();
        if (economyHUD != null)
        {
            economyHUD.goldText = goldT.GetComponent<TextMeshProUGUI>();
            economyHUD.diamondText = diaT.GetComponent<TextMeshProUGUI>();
            EditorUtility.SetDirty(economyHUD);
        }

        Debug.Log("RedesignHUD: EconomyHUD built");
    }

    // ═══════════════════════════════════════════════════════════
    //  TIME HUD: Top-left panel showing Period | Day | Time
    // ═══════════════════════════════════════════════════════════
    static void BuildTimeHUD(Canvas canvas, TMP_FontAsset font)
    {
        Transform tHud = FindChild(canvas.transform, "TimeHUD");
        if (tHud == null)
        {
            GameObject go = new GameObject("TimeHUD", typeof(RectTransform), typeof(Image));
            go.AddComponent<TimeHUD>();
            go.transform.SetParent(canvas.transform, false);
            tHud = go.transform;
            Debug.Log("RedesignHUD: Created TimeHUD GameObject");
        }

        // Ensure parented directly to Canvas (not nested under inactive parents)
        if (tHud.parent != canvas.transform)
        {
            tHud.SetParent(canvas.transform, false);
            Debug.Log("RedesignHUD: Re-parented TimeHUD to Canvas");
        }

        tHud.gameObject.SetActive(true);
        tHud.gameObject.layer = 5; // UI layer

        foreach (var lg in tHud.GetComponents<LayoutGroup>()) Object.DestroyImmediate(lg);
        foreach (var csf in tHud.GetComponents<ContentSizeFitter>()) Object.DestroyImmediate(csf);

        // Position: top-left
        var rt = tHud.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(16, -12);
        rt.sizeDelta = new Vector2(320, 48);
        rt.localScale = Vector3.one;
        FixZ(rt);

        // Background
        SetupPanelBG(tHud, PANEL_BG2_PATH);

        // Remove old DayNightIcon
        Transform oldIcon = FindChild(tHud, "DayNightIcon");
        if (oldIcon != null) Object.DestroyImmediate(oldIcon.gameObject);
        oldIcon = FindChild(canvas.transform, "DayNightIcon");
        if (oldIcon != null) Object.DestroyImmediate(oldIcon.gameObject);

        // --- PeriodText (left 35%): "Morning" with color ---
        Transform periodT = FindOrCreateChild(tHud, "PeriodText", true);
        var periodRT = periodT.GetComponent<RectTransform>();
        periodRT.anchorMin = new Vector2(0, 0);
        periodRT.anchorMax = new Vector2(0.35f, 1);
        periodRT.offsetMin = new Vector2(8, 4);
        periodRT.offsetMax = new Vector2(-2, -4);
        periodRT.localScale = Vector3.one;
        FixZ(periodRT);
        SetupTMP(periodT, font, 17, PERIOD_MORNING, TextAlignmentOptions.Center, "Morning");

        // --- DayText (center 30%): "Day 1" ---
        Transform dayT = FindOrCreateChild(tHud, "DayText", true);
        var dayRT = dayT.GetComponent<RectTransform>();
        dayRT.anchorMin = new Vector2(0.35f, 0);
        dayRT.anchorMax = new Vector2(0.65f, 1);
        dayRT.offsetMin = new Vector2(2, 4);
        dayRT.offsetMax = new Vector2(-2, -4);
        dayRT.localScale = Vector3.one;
        FixZ(dayRT);
        SetupTMP(dayT, font, 18, WARM_WHITE, TextAlignmentOptions.Center, "Day 1");

        // --- TimeText (right 35%): "6:00 AM" ---
        Transform timeT = FindOrCreateChild(tHud, "TimeText", true);
        var timeRT = timeT.GetComponent<RectTransform>();
        timeRT.anchorMin = new Vector2(0.65f, 0);
        timeRT.anchorMax = new Vector2(1, 1);
        timeRT.offsetMin = new Vector2(2, 4);
        timeRT.offsetMax = new Vector2(-8, -4);
        timeRT.localScale = Vector3.one;
        FixZ(timeRT);
        SetupTMP(timeT, font, 18, WARM_WHITE, TextAlignmentOptions.Center, "6:00 AM");

        // Separators
        SetupSeparator(tHud, "Sep1", 0.35f);
        SetupSeparator(tHud, "Sep2", 0.65f);

        // Wire up TimeHUD component references
        var timeHUD = tHud.GetComponent<TimeHUD>();
        if (timeHUD != null)
        {
            timeHUD.timeText = timeT.GetComponent<TextMeshProUGUI>();
            timeHUD.dayText = dayT.GetComponent<TextMeshProUGUI>();
            timeHUD.periodText = periodT.GetComponent<TextMeshProUGUI>();
            EditorUtility.SetDirty(timeHUD);
        }

        Debug.Log("RedesignHUD: TimeHUD built");
    }

    // ─────────── Helper Methods ───────────

    static void SetupCurrencySlot(Transform slot, TMP_FontAsset font, string iconPath, string iconName,
        Vector2 anchorMin, Vector2 anchorMax, Vector4 padding)
    {
        var slotRT = slot.GetComponent<RectTransform>();
        slotRT.anchorMin = anchorMin;
        slotRT.anchorMax = anchorMax;
        slotRT.offsetMin = new Vector2(padding.x, padding.y);
        slotRT.offsetMax = new Vector2(-padding.z, -padding.w);
        slotRT.localScale = Vector3.one;
        FixZ(slotRT);

        // Disable any background Image on the text object itself
        var slotImg = slot.GetComponent<Image>();
        if (slotImg != null) slotImg.enabled = false;

        // Text setup with left margin for icon
        var tmp = slot.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.font = font;
            tmp.fontSize = 20;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = WARM_WHITE;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.enableWordWrapping = false;
            tmp.margin = new Vector4(32, 0, 0, 0); // Space for icon
            tmp.text = "0";
        }

        // Icon
        Transform iconT = slot.Find(iconName);
        GameObject iconGO;
        if (iconT == null)
        {
            iconGO = new GameObject(iconName, typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(slot, false);
        }
        else
        {
            iconGO = iconT.gameObject;
        }

        var iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0, 0.5f);
        iconRT.anchorMax = new Vector2(0, 0.5f);
        iconRT.pivot = new Vector2(0, 0.5f);
        iconRT.anchoredPosition = new Vector2(2, 0);
        iconRT.sizeDelta = new Vector2(28, 28);
        iconRT.localScale = Vector3.one;
        FixZ(iconRT);

        var iconImg = iconGO.GetComponent<Image>();
        Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        if (spr != null)
        {
            iconImg.sprite = spr;
            iconImg.preserveAspect = true;
        }
        iconImg.color = Color.white;
        iconImg.enabled = true;
        iconImg.raycastTarget = false;
    }

    static void SetupTMP(Transform obj, TMP_FontAsset font, int fontSize, Color color,
        TextAlignmentOptions alignment, string defaultText)
    {
        var tmp = obj.GetComponent<TextMeshProUGUI>();
        if (tmp == null) return;

        // Disable any Image on the text GO
        var img = obj.GetComponent<Image>();
        if (img != null) img.enabled = false;

        tmp.font = font;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.enableWordWrapping = false;
        tmp.text = defaultText;

        // Force material sync
        if (font != null && font.material != null)
        {
            tmp.fontSharedMaterial = font.material;
        }
        tmp.SetAllDirty();
    }

    static void SetupPanelBG(Transform obj, string spritePath)
    {
        var img = obj.GetComponent<Image>();
        if (img == null) img = obj.gameObject.AddComponent<Image>();

        Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (spr != null)
        {
            img.sprite = spr;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 1f;
            img.color = Color.white;
        }
        else
        {
            img.sprite = null;
            img.color = new Color(0.2f, 0.13f, 0.08f, 0.85f);
        }
        img.enabled = true;
        img.raycastTarget = false;
    }

    static void SetupSeparator(Transform parent, string name, float xAnchor)
    {
        Transform sepT = parent.Find(name);
        GameObject sepGO;
        if (sepT == null)
        {
            sepGO = new GameObject(name, typeof(RectTransform), typeof(Image));
            sepGO.transform.SetParent(parent, false);
        }
        else
        {
            sepGO = sepT.gameObject;
        }

        var sepRT = sepGO.GetComponent<RectTransform>();
        sepRT.anchorMin = new Vector2(xAnchor, 0.15f);
        sepRT.anchorMax = new Vector2(xAnchor, 0.85f);
        sepRT.pivot = new Vector2(0.5f, 0.5f);
        sepRT.offsetMin = new Vector2(-1, 0);
        sepRT.offsetMax = new Vector2(1, 0);
        sepRT.localScale = Vector3.one;
        FixZ(sepRT);

        var sepImg = sepGO.GetComponent<Image>();
        if (sepImg == null) sepImg = sepGO.AddComponent<Image>();
        sepImg.color = new Color(0.3f, 0.2f, 0.1f, 0.5f);
        sepImg.raycastTarget = false;
    }

    static Transform FindOrCreateChild(Transform parent, string name, bool withTMP)
    {
        Transform child = parent.Find(name);
        if (child == null)
        {
            GameObject go;
            if (withTMP)
            {
                go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            }
            else
            {
                go = new GameObject(name, typeof(RectTransform));
            }
            go.transform.SetParent(parent, false);
            child = go.transform;
        }
        child.gameObject.SetActive(true);
        return child;
    }

    static Transform FindChild(Transform parent, string name)
    {
        foreach (var t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t != parent && t.name == name) return t;
        }
        return null;
    }

    static void FixZ(RectTransform rt)
    {
        Vector3 lp = rt.localPosition;
        lp.z = 0;
        rt.localPosition = lp;
    }
}
