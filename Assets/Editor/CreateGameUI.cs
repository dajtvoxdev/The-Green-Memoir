using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tạo lại toàn bộ UI game từ đầu - sạch sẽ, chuẩn xác.
/// Run: Tools > Moonlit Garden > Create Game UI
/// </summary>
public class CreateGameUI : EditorWindow
{
    [MenuItem("Tools/Moonlit Garden/Create Game UI")]
    public static void CreateUI()
    {
        if (!EditorUtility.DisplayDialog("Tạo UI mới", 
            "Thao tác này sẽ xóa Canvas cũ và tạo UI mới hoàn toàn. Tiếp tục?", 
            "Có", "Không"))
        {
            return;
        }

        Debug.Log("=== CreateGameUI: Bắt đầu tạo UI mới ===");

        // Xóa Canvas cũ nếu có
        Canvas oldCanvas = FindFirstObjectByType<Canvas>();
        if (oldCanvas != null)
        {
            Undo.DestroyObjectImmediate(oldCanvas.gameObject);
            Debug.Log("CreateGameUI: Đã xóa Canvas cũ");
        }

        // Tạo Canvas mới
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        RectTransform canvasRt = canvasObj.GetComponent<RectTransform>();
        canvasRt.anchorMin = Vector2.zero;
        canvasRt.anchorMax = Vector2.one;
        canvasRt.sizeDelta = Vector2.zero;

        Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");

        // Tạo EventSystem nếu chưa có
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
        }

        // Tạo các UI elements
        CreateTimeHUD(canvasObj.transform);
        CreateStaminaHUD(canvasObj.transform);
        CreateGoldHUD(canvasObj.transform);
        CreateInventoryHUD(canvasObj.transform);
        CreateSettingsPanel(canvasObj.transform);

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("=== CreateGameUI: Hoàn thành! ===");
    }

    private static void CreateTimeHUD(Transform canvas)
    {
        GameObject hud = new GameObject("TimeHUD", typeof(RectTransform));
        hud.transform.SetParent(canvas, false);

        RectTransform rt = hud.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(10, -10);
        rt.sizeDelta = new Vector2(250, 60);

        Image bg = hud.AddComponent<Image>();
        bg.color = new Color32(60, 40, 30, 200);
        bg.type = Image.Type.Sliced;

        // Time Text
        GameObject timeText = new GameObject("TimeText", typeof(RectTransform));
        timeText.transform.SetParent(hud.transform, false);
        RectTransform timeRt = timeText.GetComponent<RectTransform>();
        timeRt.anchorMin = new Vector2(0, 0.5f);
        timeRt.anchorMax = new Vector2(1, 1);
        timeRt.pivot = new Vector2(0.5f, 1);
        timeRt.offsetMin = new Vector2(5, 0);
        timeRt.offsetMax = new Vector2(-5, 0);

        TMP_Text timeTmp = timeText.AddComponent<TextMeshProUGUI>();
        timeTmp.text = "12:00";
        timeTmp.fontSize = 24;
        timeTmp.alignment = TextAlignmentOptions.Center;
        timeTmp.color = new Color32(255, 245, 220, 255);

        // Day Text
        GameObject dayText = new GameObject("DayText", typeof(RectTransform));
        dayText.transform.SetParent(hud.transform, false);
        RectTransform dayRt = dayText.GetComponent<RectTransform>();
        dayRt.anchorMin = new Vector2(0, 0);
        dayRt.anchorMax = new Vector2(1, 0.5f);
        dayRt.pivot = new Vector2(0.5f, 0);
        dayRt.offsetMin = new Vector2(5, 0);
        dayRt.offsetMax = new Vector2(-5, 0);

        TMP_Text dayTmp = dayText.AddComponent<TextMeshProUGUI>();
        dayTmp.text = "Ngày 1";
        dayTmp.fontSize = 16;
        dayTmp.alignment = TextAlignmentOptions.Center;
        dayTmp.color = new Color32(220, 200, 170, 255);

        // Thêm TimeHUD script
        TimeHUD timeHud = hud.AddComponent<TimeHUD>();
        timeHud.timeText = timeTmp;
        timeHud.dayText = dayTmp;

        Debug.Log("CreateGameUI: Đã tạo TimeHUD");
    }

    private static void CreateStaminaHUD(Transform canvas)
    {
        GameObject hud = new GameObject("StaminaHUD", typeof(RectTransform));
        hud.transform.SetParent(canvas, false);

        RectTransform rt = hud.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(10, -80);
        rt.sizeDelta = new Vector2(200, 40);

        Image bg = hud.AddComponent<Image>();
        bg.color = new Color32(60, 40, 30, 200);
        bg.type = Image.Type.Sliced;

        // Slider
        GameObject sliderObj = new GameObject("StaminaSlider", typeof(RectTransform));
        sliderObj.transform.SetParent(hud.transform, false);
        RectTransform sliderRt = sliderObj.GetComponent<RectTransform>();
        sliderRt.anchorMin = new Vector2(0, 0);
        sliderRt.anchorMax = new Vector2(1, 1);
        sliderRt.offsetMin = new Vector2(10, 5);
        sliderRt.offsetMax = new Vector2(-10, -5);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;

        // Fill
        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(sliderObj.transform, false);
        Image fillImg = fillObj.GetComponent<Image>();
        fillImg.color = new Color32(100, 200, 100, 255);

        slider.fillRect = fillObj.GetComponent<RectTransform>();
        slider.targetGraphic = fillImg;

        Debug.Log("CreateGameUI: Đã tạo StaminaHUD");
    }

    private static void CreateGoldHUD(Transform canvas)
    {
        GameObject hud = new GameObject("GoldHUD", typeof(RectTransform));
        hud.transform.SetParent(canvas, false);

        RectTransform rt = hud.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-10, -10);
        rt.sizeDelta = new Vector2(150, 40);

        Image bg = hud.AddComponent<Image>();
        bg.color = new Color32(60, 40, 30, 200);
        bg.type = Image.Type.Sliced;

        // Icon
        GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(hud.transform, false);
        RectTransform iconRt = icon.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0, 0.5f);
        iconRt.anchorMax = new Vector2(0, 0.5f);
        iconRt.pivot = new Vector2(0, 0.5f);
        iconRt.anchoredPosition = new Vector2(5, 0);
        iconRt.sizeDelta = new Vector2(30, 30);

        Image iconImg = icon.GetComponent<Image>();
        iconImg.color = new Color32(255, 215, 0, 255);

        // Text
        GameObject goldText = new GameObject("GoldText", typeof(RectTransform));
        goldText.transform.SetParent(hud.transform, false);
        RectTransform textRt = goldText.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0, 0);
        textRt.anchorMax = new Vector2(1, 1);
        textRt.offsetMin = new Vector2(40, 0);
        textRt.offsetMax = new Vector2(-5, 0);

        TMP_Text tmp = goldText.AddComponent<TextMeshProUGUI>();
        tmp.text = "100";
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.color = new Color32(255, 245, 220, 255);

        Debug.Log("CreateGameUI: Đã tạo GoldHUD");
    }

    private static void CreateInventoryHUD(Transform canvas)
    {
        GameObject hud = new GameObject("InventoryHUD", typeof(RectTransform));
        hud.transform.SetParent(canvas, false);

        RectTransform rt = hud.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 10);
        rt.sizeDelta = new Vector2(600, 80);

        Image bg = hud.AddComponent<Image>();
        bg.color = new Color32(60, 40, 30, 200);
        bg.type = Image.Type.Sliced;

        Debug.Log("CreateGameUI: Đã tạo InventoryHUD");
    }

    private static void CreateSettingsPanel(Transform canvas)
    {
        GameObject panel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(CanvasGroup));
        panel.transform.SetParent(canvas, false);
        panel.SetActive(false);

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(450, 350);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color32(80, 60, 40, 250);
        bg.type = Image.Type.Sliced;

        // PanelBase
        PanelBase panelBase = panel.AddComponent<PanelBase>();
        panelBase.panelId = "settings";
        panelBase.pauseGameWhenOpen = true;
        panelBase.closeOnEscape = true;

        // SettingsPanel script
        SettingsPanel settingsPanel = panel.AddComponent<SettingsPanel>();
        settingsPanel.panelRoot = panel;
        settingsPanel.toggleKey = KeyCode.Escape;

        // Title
        GameObject title = new GameObject("Title", typeof(RectTransform));
        title.transform.SetParent(panel.transform, false);
        RectTransform titleRt = title.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1);
        titleRt.anchoredPosition = new Vector2(0, -10);
        titleRt.sizeDelta = new Vector2(0, 40);

        TMP_Text titleTmp = title.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "CÀI ĐẶT";
        titleTmp.fontSize = 28;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = new Color32(255, 245, 220, 255);

        // Close Button
        GameObject closeBtn = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtn.transform.SetParent(panel.transform, false);
        RectTransform btnRt = closeBtn.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0);
        btnRt.anchorMax = new Vector2(0.5f, 0);
        btnRt.pivot = new Vector2(0.5f, 0);
        btnRt.anchoredPosition = new Vector2(0, 20);
        btnRt.sizeDelta = new Vector2(120, 40);

        Image btnImg = closeBtn.GetComponent<Image>();
        btnImg.color = new Color32(150, 100, 60, 255);

        Button btn = closeBtn.GetComponent<Button>();
        btn.onClick.AddListener(() => settingsPanel.ClosePanel());

        Debug.Log("CreateGameUI: Đã tạo SettingsPanel");
    }
}
