using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Fix SettingsPanel UI issues: wrong scale, missing PanelBase, poor layout.
/// Run: Tools > Moonlit Garden > Fix Settings Panel UI
/// </summary>
public class FixSettingsPanelUI : EditorWindow
{
    private const string RUSTIC_ROOT = "Assets/Rustic UI/UI-Singles/";
    private const string SP_PANEL_LG = RUSTIC_ROOT + "UI - 1.png";
    private const string SP_BUTTON = RUSTIC_ROOT + "UI - 15.png";
    private const string SP_SLIDER_BG = RUSTIC_ROOT + "UI - 40.png";
    private const string SP_SLIDER_FILL = RUSTIC_ROOT + "UI - 30.png";
    private const string SP_KNOB = RUSTIC_ROOT + "UI - 20.png";

    private static readonly Color32 COL_PANEL_BG = new Color32(120, 80, 50, 255);
    private static readonly Color32 COL_TEXT = new Color32(255, 245, 220, 255);
    private static readonly Color32 COL_TEXT_DIM = new Color32(220, 200, 170, 255);

    [MenuItem("Tools/Moonlit Garden/Fix Settings Panel UI")]
    public static void FixSettingsPanel()
    {
        Debug.Log("=== FixSettingsPanelUI: Starting ===");

        // Find Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("FixSettingsPanelUI: No Canvas found!");
            return;
        }

        // Find and destroy old SettingsPanel
        Transform oldPanel = FindChildRecursive(canvas.transform, "SettingsPanel");
        if (oldPanel != null)
        {
            Undo.DestroyObjectImmediate(oldPanel.gameObject);
            Debug.Log("FixSettingsPanelUI: Removed old SettingsPanel");
        }

        Transform oldController = FindChildRecursive(canvas.transform, "SettingsPanelController");
        if (oldController != null)
        {
            Undo.DestroyObjectImmediate(oldController.gameObject);
            Debug.Log("FixSettingsPanelUI: Removed old SettingsPanelController");
        }

        // Create new SettingsPanel
        CreateNewSettingsPanel(canvas.transform);

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("=== FixSettingsPanelUI: Complete! ===");
    }

    private static void CreateNewSettingsPanel(Transform canvasTransform)
    {
        // Create panel root with PanelBase
        GameObject panelRoot = new GameObject("SettingsPanel", typeof(RectTransform), typeof(CanvasGroup));
        panelRoot.transform.SetParent(canvasTransform, false);
        Undo.RegisterCreatedObjectUndo(panelRoot, "Create SettingsPanel");

        // Add PanelBase component
        PanelBase panelBase = panelRoot.AddComponent<PanelBase>();
        panelBase.panelId = "settings";
        panelBase.pauseGameWhenOpen = true;
        panelBase.closeOnEscape = true;
        panelBase.fadeSpeed = 8f;

        // Add SettingsPanel component (the controller)
        SettingsPanel settingsPanel = panelRoot.AddComponent<SettingsPanel>();

        // Setup RectTransform - center of screen, fixed size
        RectTransform rt = panelRoot.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(450, 350);

        // Background Image
        Image bgImg = panelRoot.AddComponent<Image>();
        bgImg.color = COL_PANEL_BG;
        bgImg.type = Image.Type.Sliced;
        Sprite panelSprite = LoadSprite(SP_PANEL_LG);
        if (panelSprite != null)
        {
            bgImg.sprite = panelSprite;
        }

        // Font setup - use default if Cherry Bomb has issues
        TMP_FontAsset font = GetFont();

        // Title
        GameObject titleObj = CreateText("Title", panelRoot.transform, "CÀI ĐẶT", 24, FontStyles.Bold);
        SetAnchoredPosition(titleObj, 0, 140);

        // BGM Row
        GameObject bgmLabel = CreateText("BGMLabel", panelRoot.transform, "Nhạc nền", 16);
        SetAnchoredPosition(bgmLabel, -120, 80);

        GameObject bgmSliderObj = CreateSlider("BGMSlider", panelRoot.transform);
        SetAnchoredPosition(bgmSliderObj, 80, 80);
        bgmSliderObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 30);

        GameObject bgmValue = CreateText("BGMValue", panelRoot.transform, "100%", 14);
        SetAnchoredPosition(bgmValue, 200, 80);
        ((TMP_Text)bgmValue.GetComponent<TMP_Text>()).color = (Color)COL_TEXT_DIM;

        // SFX Row
        GameObject sfxLabel = CreateText("SFXLabel", panelRoot.transform, "Hiệu ứng âm thanh", 16);
        SetAnchoredPosition(sfxLabel, -120, 30);

        GameObject sfxSliderObj = CreateSlider("SFXSlider", panelRoot.transform);
        SetAnchoredPosition(sfxSliderObj, 80, 30);
        sfxSliderObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 30);

        GameObject sfxValue = CreateText("SFXValue", panelRoot.transform, "100%", 14);
        SetAnchoredPosition(sfxValue, 200, 30);
        ((TMP_Text)sfxValue.GetComponent<TMP_Text>()).color = (Color)COL_TEXT_DIM;

        // Fullscreen Toggle
        GameObject fullscreenToggleObj = CreateToggle("FullscreenToggle", panelRoot.transform, "Toàn màn hình");
        SetAnchoredPosition(fullscreenToggleObj, 0, -40);

        // Close Button
        GameObject closeBtnObj = CreateButton("CloseButton", panelRoot.transform, "ĐÓNG");
        SetAnchoredPosition(closeBtnObj, 0, -120);

        // Wire up SettingsPanel references
        settingsPanel.panelRoot = panelRoot;
        settingsPanel.bgmSlider = bgmSliderObj.GetComponent<Slider>();
        settingsPanel.sfxSlider = sfxSliderObj.GetComponent<Slider>();
        settingsPanel.fullscreenToggle = fullscreenToggleObj.GetComponent<Toggle>();
        settingsPanel.bgmValueText = bgmValue.GetComponent<TMP_Text>();
        settingsPanel.sfxValueText = sfxValue.GetComponent<TMP_Text>();
        settingsPanel.toggleKey = KeyCode.Escape;

        // Wire close button
        Button closeBtn = closeBtnObj.GetComponent<Button>();
        closeBtn.onClick.AddListener(() => settingsPanel.ClosePanel());

        // Start hidden
        panelRoot.SetActive(false);

        Debug.Log("FixSettingsPanelUI: Created new SettingsPanel with proper structure");
    }

    private static GameObject CreateText(string name, Transform parent, string text, int fontSize, FontStyles style = FontStyles.Normal)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        TMP_Text tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = (Color)COL_TEXT;
        tmp.alignment = TextAlignmentOptions.Center;

        TMP_FontAsset font = GetFont();
        if (font != null) tmp.font = font;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 40);

        return obj;
    }

    private static GameObject CreateSlider(string name, Transform parent)
    {
        GameObject sliderObj = new GameObject(name, typeof(RectTransform));
        sliderObj.transform.SetParent(parent, false);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;

        // Background
        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(sliderObj.transform, false);
        Image bgImg = bg.GetComponent<Image>();
        bgImg.color = new Color32(60, 40, 30, 255);
        bgImg.type = Image.Type.Sliced;
        Sprite bgSprite = LoadSprite(SP_SLIDER_BG);
        if (bgSprite != null) bgImg.sprite = bgSprite;
        StretchToParent(bg);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRt = fillArea.GetComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.sizeDelta = new Vector2(-20, -10);

        // Fill
        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.GetComponent<Image>();
        fillImg.color = new Color32(200, 150, 80, 255);
        fillImg.type = Image.Type.Sliced;
        Sprite fillSprite = LoadSprite(SP_SLIDER_FILL);
        if (fillSprite != null) fillImg.sprite = fillSprite;
        StretchToParent(fill);

        // Handle
        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(sliderObj.transform, false);
        RectTransform handleRt = handle.GetComponent<RectTransform>();
        handleRt.anchorMin = new Vector2(0.5f, 0.5f);
        handleRt.anchorMax = new Vector2(0.5f, 0.5f);
        handleRt.sizeDelta = new Vector2(24, 24);
        Image handleImg = handle.GetComponent<Image>();
        Sprite knobSprite = LoadSprite(SP_KNOB);
        if (knobSprite != null) handleImg.sprite = knobSprite;

        // Wire slider
        slider.targetGraphic = handleImg;
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handleRt;

        return sliderObj;
    }

    private static GameObject CreateToggle(string name, Transform parent, string label)
    {
        GameObject toggleObj = new GameObject(name, typeof(RectTransform));
        toggleObj.transform.SetParent(parent, false);

        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.isOn = Screen.fullScreen;

        RectTransform rt = toggleObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 40);

        // Background
        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(toggleObj.transform, false);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0.5f);
        bgRt.anchorMax = new Vector2(0, 0.5f);
        bgRt.pivot = new Vector2(0, 0.5f);
        bgRt.anchoredPosition = Vector2.zero;
        bgRt.sizeDelta = new Vector2(30, 30);
        Image bgImg = bg.GetComponent<Image>();
        bgImg.color = new Color32(80, 60, 40, 255);

        // Checkmark
        GameObject checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        checkmark.transform.SetParent(bg.transform, false);
        StretchToParent(checkmark);
        Image checkImg = checkmark.GetComponent<Image>();
        checkImg.color = new Color32(200, 180, 120, 255);

        // Label
        GameObject labelObj = CreateText("Label", toggleObj.transform, label, 16);
        RectTransform labelRt = labelObj.GetComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0, 0.5f);
        labelRt.anchorMax = new Vector2(0, 0.5f);
        labelRt.pivot = new Vector2(0, 0.5f);
        labelRt.anchoredPosition = new Vector2(40, 0);

        toggle.targetGraphic = bgImg;
        toggle.graphic = checkImg;

        return toggleObj;
    }

    private static GameObject CreateButton(string name, Transform parent, string text)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120, 45);

        Image img = btnObj.GetComponent<Image>();
        img.color = new Color32(150, 100, 60, 255);
        img.type = Image.Type.Sliced;
        Sprite btnSprite = LoadSprite(SP_BUTTON);
        if (btnSprite != null) img.sprite = btnSprite;

        // Text
        GameObject textObj = CreateText("Text", btnObj.transform, text, 16, FontStyles.Bold);
        textObj.GetComponent<TMP_Text>().color = new Color32(255, 255, 255, 255);
        StretchToParent(textObj);

        return btnObj;
    }

    private static void SetAnchoredPosition(GameObject obj, float x, float y)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
    }

    private static void StretchToParent(GameObject obj)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

private static TMP_FontAsset GetFont()
    {
        // Cherry Bomb font has atlas texture issues, use default instead
        return TMP_Settings.defaultFontAsset;
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        Transform result = parent.Find(name);
        if (result != null) return result;

        foreach (Transform child in parent)
        {
            result = FindChildRecursive(child, name);
            if (result != null) return result;
        }

        return null;
    }
}
