using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Creates and wires the UpdatePromptUI panel in the LoginScene.
/// Run: Tools > Moonlit Garden > Setup Update Prompt UI
/// </summary>
public class SetupUpdatePromptUI : EditorWindow
{
    [MenuItem("Tools/Moonlit Garden/Setup Update Prompt UI")]
    public static void Setup()
    {
        Debug.Log("=== SetupUpdatePromptUI: Starting ===");

        // Find Canvas in the scene
        Canvas canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null)
        {
            Debug.LogError("SetupUpdatePromptUI: No Canvas found in scene!");
            return;
        }

        // Find FirebaseLoginManager
        FirebaseLoginManager loginManager = Object.FindFirstObjectByType<FirebaseLoginManager>(FindObjectsInactive.Include);
        if (loginManager == null)
        {
            Debug.LogError("SetupUpdatePromptUI: No FirebaseLoginManager found in scene!");
            return;
        }

        // Check if UpdatePromptUI already exists
        UpdatePromptUI existing = Object.FindFirstObjectByType<UpdatePromptUI>(FindObjectsInactive.Include);
        if (existing != null)
        {
            Debug.Log("SetupUpdatePromptUI: UpdatePromptUI already exists, updating references.");
            WireLoginManager(loginManager, existing);
            return;
        }

        // === Create the panel hierarchy ===

        // Root: UpdatePromptPanel (full-screen overlay)
        GameObject panelRoot = new GameObject("UpdatePromptPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        panelRoot.transform.SetParent(canvas.transform, false);
        Undo.RegisterCreatedObjectUndo(panelRoot, "Create UpdatePromptPanel");

        RectTransform rootRT = panelRoot.GetComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero;
        rootRT.offsetMax = Vector2.zero;

        // Semi-transparent dark overlay
        Image overlayImage = panelRoot.GetComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.85f);
        overlayImage.raycastTarget = true;

        // Inner container (centered card)
        GameObject card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline));
        card.transform.SetParent(panelRoot.transform, false);

        RectTransform cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.5f, 0.5f);
        cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(500, 360);
        cardRT.anchoredPosition = Vector2.zero;

        Image cardBg = card.GetComponent<Image>();
        cardBg.color = new Color(0.12f, 0.14f, 0.22f, 0.97f);

        Outline cardOutline = card.GetComponent<Outline>();
        cardOutline.effectColor = new Color(0.3f, 0.8f, 0.6f, 0.5f);
        cardOutline.effectDistance = new Vector2(2, -2);

        // Vertical layout
        VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(24, 24, 20, 20);
        vlg.spacing = 12;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        // === Title ===
        GameObject titleGO = CreateTMPText("TitleText", card.transform, "Cập nhật có sẵn!", 22, FontStyles.Bold,
            new Color(0.3f, 0.9f, 0.6f), TextAlignmentOptions.Center, 36);

        // === Message ===
        GameObject messageGO = CreateTMPText("MessageText", card.transform, "Phiên bản mới đang chờ bạn.", 15, FontStyles.Normal,
            new Color(0.85f, 0.85f, 0.9f), TextAlignmentOptions.Center, 70);

        // === Progress Bar ===
        GameObject progressBarGO = CreateProgressBar(card.transform);

        // === Progress Text ===
        GameObject progressTextGO = CreateTMPText("ProgressText", card.transform, "0%", 14, FontStyles.Normal,
            new Color(1f, 0.95f, 0.3f), TextAlignmentOptions.Center, 24);

        // === Status Text ===
        GameObject statusTextGO = CreateTMPText("StatusText", card.transform, "", 13, FontStyles.Italic,
            Color.white, TextAlignmentOptions.Center, 24);

        // === Button row ===
        GameObject buttonRow = new GameObject("ButtonRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        buttonRow.transform.SetParent(card.transform, false);

        HorizontalLayoutGroup hlg = buttonRow.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 16;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        LayoutElement rowLE = buttonRow.AddComponent<LayoutElement>();
        rowLE.preferredHeight = 44;

        // Update button
        GameObject updateBtnGO = CreateButton("UpdateButton", buttonRow.transform, "Cập nhật ngay",
            new Color(0.2f, 0.7f, 0.45f), new Vector2(180, 40));

        // Cancel button
        GameObject cancelBtnGO = CreateButton("CancelButton", buttonRow.transform, "Đóng",
            new Color(0.4f, 0.35f, 0.35f), new Vector2(120, 40));

        // === Wire UpdatePromptUI component ===
        UpdatePromptUI promptUI = panelRoot.AddComponent<UpdatePromptUI>();
        promptUI.panelRoot = panelRoot;
        promptUI.titleText = titleGO.GetComponent<TMP_Text>();
        promptUI.messageText = messageGO.GetComponent<TMP_Text>();
        promptUI.progressBar = progressBarGO.GetComponent<Slider>();
        promptUI.progressText = progressTextGO.GetComponent<TMP_Text>();
        promptUI.statusText = statusTextGO.GetComponent<TMP_Text>();
        promptUI.updateButton = updateBtnGO.GetComponent<Button>();
        promptUI.updateButtonText = updateBtnGO.GetComponentInChildren<TMP_Text>();
        promptUI.cancelButton = cancelBtnGO.GetComponent<Button>();

        // Wire to login manager
        WireLoginManager(loginManager, promptUI);

        // Start hidden
        panelRoot.SetActive(false);

        EditorUtility.SetDirty(panelRoot);
        EditorUtility.SetDirty(loginManager);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("=== SetupUpdatePromptUI: Complete! ===");
    }

    private static void WireLoginManager(FirebaseLoginManager loginManager, UpdatePromptUI promptUI)
    {
        Undo.RecordObject(loginManager, "Wire UpdatePromptUI");
        loginManager.updatePromptUI = promptUI;
        EditorUtility.SetDirty(loginManager);
        Debug.Log("SetupUpdatePromptUI: Wired UpdatePromptUI to FirebaseLoginManager");
    }

    private static GameObject CreateTMPText(string name, Transform parent, string text, float fontSize,
        FontStyles style, Color color, TextAlignmentOptions alignment, float preferredHeight)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        TMP_Text tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.raycastTarget = false;

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = preferredHeight;

        return go;
    }

    private static GameObject CreateProgressBar(Transform parent)
    {
        // Slider root
        GameObject sliderGO = new GameObject("ProgressBar", typeof(RectTransform), typeof(Slider));
        sliderGO.transform.SetParent(parent, false);

        Slider slider = sliderGO.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.interactable = false;

        LayoutElement le = sliderGO.AddComponent<LayoutElement>();
        le.preferredHeight = 20;

        // Background
        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(sliderGO.transform, false);
        RectTransform bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f);

        // Fill area
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRT = fillArea.GetComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0, 0.25f);
        fillAreaRT.anchorMax = new Vector2(1, 0.75f);
        fillAreaRT.offsetMin = new Vector2(5, 0);
        fillAreaRT.offsetMax = new Vector2(-5, 0);

        // Fill
        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        fill.GetComponent<Image>().color = new Color(0.3f, 0.85f, 0.55f);

        slider.fillRect = fillRT;

        return sliderGO;
    }

    private static GameObject CreateButton(string name, Transform parent, string label, Color bgColor, Vector2 size)
    {
        GameObject btnGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnGO.transform.SetParent(parent, false);

        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.sizeDelta = size;

        Image img = btnGO.GetComponent<Image>();
        img.color = bgColor;

        Outline outline = btnGO.GetComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.15f);
        outline.effectDistance = new Vector2(1, -1);

        // Button text
        GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(btnGO.transform, false);

        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(4, 2);
        textRT.offsetMax = new Vector2(-4, -2);

        TMP_Text tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 16;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        return btnGO;
    }
}
