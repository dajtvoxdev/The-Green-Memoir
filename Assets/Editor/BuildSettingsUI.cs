using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Builds the Settings panel UI in PlayScene with:
///   - BGM volume slider + mute toggle
///   - SFX volume slider + mute toggle
///   - Fullscreen toggle
///   - Logout button
///   - Close button
/// Wires all references to the SettingsPanel component.
///
/// Run via: Tools > Build Settings UI
/// </summary>
public class BuildSettingsUI
{
    // Vietnamese farmland color palette
    private static readonly Color panelBg    = new Color(0.12f, 0.10f, 0.08f, 0.92f);
    private static readonly Color headerCol  = new Color(0.96f, 0.87f, 0.50f);
    private static readonly Color labelCol   = new Color(0.90f, 0.88f, 0.82f);
    private static readonly Color sliderBg   = new Color(0.25f, 0.22f, 0.18f);
    private static readonly Color sliderFill = new Color(0.45f, 0.72f, 0.30f);
    private static readonly Color toggleOn   = new Color(0.45f, 0.72f, 0.30f);
    private static readonly Color btnClose   = new Color(0.80f, 0.25f, 0.20f);
    private static readonly Color btnLogout  = new Color(0.85f, 0.30f, 0.25f);
    private static readonly Color btnLogoutHover = new Color(0.95f, 0.40f, 0.35f);

    [MenuItem("Tools/Build Settings UI")]
    public static void Build()
    {
        // Find SettingsPanel in scene — search all objects including inactive
        SettingsPanel settingsPanel = null;
        foreach (var sp in Resources.FindObjectsOfTypeAll<SettingsPanel>())
        {
            // Skip assets (only want scene objects)
            if (EditorUtility.IsPersistent(sp)) continue;
            settingsPanel = sp;
            break;
        }
        if (settingsPanel == null)
        {
            Debug.LogError("BuildSettingsUI: SettingsPanel component not found! Add it to a GameObject first.");
            return;
        }

        Debug.Log($"BuildSettingsUI: Found SettingsPanel on '{settingsPanel.gameObject.name}'");

        // Find or create Canvas
        Canvas canvas = null;
        foreach (var c in Resources.FindObjectsOfTypeAll<Canvas>())
        {
            if (EditorUtility.IsPersistent(c)) continue;
            canvas = c;
            break;
        }
        if (canvas == null)
        {
            Debug.LogError("BuildSettingsUI: No Canvas found in scene!");
            return;
        }

        // Clean up old panel root if exists
        Transform oldPanel = settingsPanel.transform.Find("SettingsPanelRoot");
        if (oldPanel != null)
        {
            Object.DestroyImmediate(oldPanel.gameObject);
        }

        // Create root panel
        var rootGO = CreatePanel("SettingsPanelRoot", settingsPanel.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(380f, 480f), panelBg);
        var root = rootGO.transform;

        // Title
        CreateText("Title", root, "Cài đặt", 26, headerCol, TextAlignmentOptions.Center,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -10f), new Vector2(0f, -50f));

        // Separator line
        var sep1 = CreatePanel("Sep1", root,
            new Vector2(0.05f, 1f), new Vector2(0.95f, 1f),
            Vector2.zero, new Color(1f, 1f, 1f, 0.15f));
        var sep1RT = sep1.GetComponent<RectTransform>();
        sep1RT.anchoredPosition = new Vector2(0f, -55f);
        sep1RT.sizeDelta = new Vector2(0f, 2f);

        float yOffset = -70f;

        // === BGM Section ===
        CreateText("LblBGM", root, "Nhạc nền", 18, labelCol, TextAlignmentOptions.Left,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(20f, yOffset), new Vector2(-20f, yOffset - 30f));
        yOffset -= 30f;

        // BGM slider
        var bgmSlider = CreateSlider("BGMSlider", root, yOffset);
        yOffset -= 40f;

        // BGM value text
        var bgmValueText = CreateText("BGMValue", root, "50%", 14, labelCol, TextAlignmentOptions.Right,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(20f, yOffset + 40f), new Vector2(-20f, yOffset + 40f - 25f));

        // BGM toggle
        var bgmToggle = CreateToggle("BGMToggle", root, "Bật nhạc nền", yOffset);
        yOffset -= 35f;

        // === SFX Section ===
        yOffset -= 10f;
        CreateText("LblSFX", root, "Hiệu ứng âm thanh", 18, labelCol, TextAlignmentOptions.Left,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(20f, yOffset), new Vector2(-20f, yOffset - 30f));
        yOffset -= 30f;

        // SFX slider
        var sfxSlider = CreateSlider("SFXSlider", root, yOffset);
        yOffset -= 40f;

        // SFX value text
        var sfxValueText = CreateText("SFXValue", root, "70%", 14, labelCol, TextAlignmentOptions.Right,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(20f, yOffset + 40f), new Vector2(-20f, yOffset + 40f - 25f));

        // SFX toggle
        var sfxToggle = CreateToggle("SFXToggle", root, "Bật hiệu ứng âm thanh", yOffset);
        yOffset -= 35f;

        // Separator
        yOffset -= 10f;
        var sep2 = CreatePanel("Sep2", root,
            new Vector2(0.05f, 1f), new Vector2(0.95f, 1f),
            Vector2.zero, new Color(1f, 1f, 1f, 0.15f));
        var sep2RT = sep2.GetComponent<RectTransform>();
        sep2RT.anchoredPosition = new Vector2(0f, yOffset);
        sep2RT.sizeDelta = new Vector2(0f, 2f);
        yOffset -= 15f;

        // Fullscreen toggle
        var fullscreenToggle = CreateToggle("FullscreenToggle", root, "Toàn màn hình", yOffset);
        yOffset -= 50f;

        // Separator
        var sep3 = CreatePanel("Sep3", root,
            new Vector2(0.05f, 1f), new Vector2(0.95f, 1f),
            Vector2.zero, new Color(1f, 1f, 1f, 0.15f));
        var sep3RT = sep3.GetComponent<RectTransform>();
        sep3RT.anchoredPosition = new Vector2(0f, yOffset + 10f);
        sep3RT.sizeDelta = new Vector2(0f, 2f);

        // Logout button
        var logoutBtn = CreateButton("LogoutButton", root, "Đăng xuất", yOffset, btnLogout);
        yOffset -= 50f;

        // Close button
        var closeBtn = CreateButton("CloseButton", root, "Đóng", yOffset, btnClose);

        // Wire references to SettingsPanel
        var so = new SerializedObject(settingsPanel);

        SetRef(so, "bgmSlider", bgmSlider.GetComponent<Slider>());
        SetRef(so, "sfxSlider", sfxSlider.GetComponent<Slider>());
        SetRef(so, "bgmValueText", bgmValueText.GetComponent<TMP_Text>());
        SetRef(so, "sfxValueText", sfxValueText.GetComponent<TMP_Text>());
        SetRef(so, "bgmToggle", bgmToggle.GetComponent<Toggle>());
        SetRef(so, "sfxToggle", sfxToggle.GetComponent<Toggle>());
        SetRef(so, "fullscreenToggle", fullscreenToggle.GetComponent<Toggle>());
        SetRef(so, "logoutButton", logoutBtn.GetComponent<Button>());

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(settingsPanel);

        // Wire close button to ClosePanel
        var closeBtnComp = closeBtn.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            closeBtnComp.onClick,
            settingsPanel.ClosePanel);

        EditorUtility.SetDirty(settingsPanel);
        Debug.Log("BuildSettingsUI: Settings panel built and wired successfully!");
    }

    private static void SetRef(SerializedObject so, string propName, Object value)
    {
        var prop = so.FindProperty(propName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
        }
        else
        {
            Debug.LogWarning($"BuildSettingsUI: Property '{propName}' not found on SettingsPanel");
        }
    }

    private static GameObject CreatePanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = Vector2.zero;

        var img = go.GetComponent<Image>();
        img.color = color;

        return go;
    }

    private static GameObject CreateText(string name, Transform parent,
        string text, float fontSize, Color color, TextAlignmentOptions alignment,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(offsetMin.x, offsetMax.y);
        rt.offsetMax = new Vector2(offsetMax.x, offsetMin.y);

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;

        return go;
    }

    private static GameObject CreateSlider(string name, Transform parent, float yPos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = new Vector2(20f, yPos - 20f);
        rt.offsetMax = new Vector2(-70f, yPos);

        // Background
        var bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(go.transform, false);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        bgGO.GetComponent<Image>().color = sliderBg;

        // Fill area
        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(go.transform, false);
        var fillAreaRT = fillArea.GetComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero;
        fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = new Vector2(5f, 5f);
        fillAreaRT.offsetMax = new Vector2(-5f, -5f);

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        var fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(0f, 1f);
        fillRT.sizeDelta = Vector2.zero;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        fill.GetComponent<Image>().color = sliderFill;

        // Handle slide area
        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(go.transform, false);
        var handleAreaRT = handleArea.GetComponent<RectTransform>();
        handleAreaRT.anchorMin = Vector2.zero;
        handleAreaRT.anchorMax = Vector2.one;
        handleAreaRT.offsetMin = new Vector2(10f, 0f);
        handleAreaRT.offsetMax = new Vector2(-10f, 0f);

        var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        var handleRT = handle.GetComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(20f, 0f);
        handle.GetComponent<Image>().color = Color.white;

        // Wire slider
        var slider = go.GetComponent<Slider>();
        slider.fillRect = fillRT;
        slider.handleRect = handleRT;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.5f;
        slider.wholeNumbers = false;

        return go;
    }

    private static GameObject CreateToggle(string name, Transform parent, string label, float yPos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Toggle));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = new Vector2(20f, yPos - 28f);
        rt.offsetMax = new Vector2(-20f, yPos);

        // Checkbox background
        var bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(go.transform, false);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0f, 0.5f);
        bgRT.anchorMax = new Vector2(0f, 0.5f);
        bgRT.anchoredPosition = new Vector2(12f, 0f);
        bgRT.sizeDelta = new Vector2(24f, 24f);
        bgGO.GetComponent<Image>().color = sliderBg;

        // Checkmark
        var checkGO = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        checkGO.transform.SetParent(bgGO.transform, false);
        var checkRT = checkGO.GetComponent<RectTransform>();
        checkRT.anchorMin = new Vector2(0.15f, 0.15f);
        checkRT.anchorMax = new Vector2(0.85f, 0.85f);
        checkRT.offsetMin = Vector2.zero;
        checkRT.offsetMax = Vector2.zero;
        checkGO.GetComponent<Image>().color = toggleOn;

        // Label
        var labelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGO.transform.SetParent(go.transform, false);
        var labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = new Vector2(36f, 0f);
        labelRT.offsetMax = Vector2.zero;

        var tmp = labelGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 16;
        tmp.color = labelCol;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;

        // Wire toggle
        var toggle = go.GetComponent<Toggle>();
        toggle.targetGraphic = bgGO.GetComponent<Image>();
        toggle.graphic = checkGO.GetComponent<Image>();
        toggle.isOn = true;

        return go;
    }

    private static GameObject CreateButton(string name, Transform parent, string label, float yPos, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.1f, 1f);
        rt.anchorMax = new Vector2(0.9f, 1f);
        rt.offsetMin = new Vector2(0f, yPos - 38f);
        rt.offsetMax = new Vector2(0f, yPos);

        var img = go.GetComponent<Image>();
        img.color = color;

        var btn = go.GetComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 1.15f;
        colors.pressedColor = color * 0.85f;
        btn.colors = colors;

        // Text
        var textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(go.transform, false);
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 18;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return go;
    }
}
