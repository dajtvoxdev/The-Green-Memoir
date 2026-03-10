using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor Script tạo toàn bộ UI cho Scene Loading và gắn tham chiếu vào Script LoadingScene.
/// Chạy 1 lần rồi xóa.
/// </summary>
public class SetupLoadingSceneUI
{
    [MenuItem("Tools/SetupLoadingSceneUI")]
    public static void Setup()
    {
        // Tìm Canvas trong Scene
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas trong Scene!");
            return;
        }

        // Cấu hình Canvas
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // Cấu hình Camera
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(30, 25, 20, 255); // Nâu rất tối
        }

        // === TẠO UI ELEMENTS ===

        // 1. Background Panel (phủ toàn màn hình, gradient tối)
        GameObject bgPanel = CreateUIElement("BackgroundPanel", canvas.transform);
        RectTransform bgRt = bgPanel.GetComponent<RectTransform>();
        StretchFull(bgRt);
        Image bgImg = bgPanel.AddComponent<Image>();
        bgImg.color = new Color32(35, 28, 22, 255); // Nâu tối ấm áp

        // 2. Title Text - "Đang tải..."
        GameObject titleObj = CreateUIElement("TitleText", canvas.transform);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.7f);
        titleRt.anchorMax = new Vector2(0.5f, 0.7f);
        titleRt.pivot = new Vector2(0.5f, 0.5f);
        titleRt.anchoredPosition = Vector2.zero;
        titleRt.sizeDelta = new Vector2(600, 80);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "Moonlit Garden";
        titleText.fontSize = 56;
        titleText.color = new Color32(255, 220, 150, 255); // Vàng ấm
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
        Font cherryFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/UI/Fonts/Cherry_Bomb_One/CherryBombOne-Regular.ttf");
        if (cherryFont == null) cherryFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (cherryFont == null) cherryFont = Font.CreateDynamicFontFromOSFont("Arial", 32);

        // 3. Loading Icon / Decorative element (hình coin xoay)
        GameObject iconObj = CreateUIElement("LoadingIcon", canvas.transform);
        RectTransform iconRt = iconObj.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.5f, 0.55f);
        iconRt.anchorMax = new Vector2(0.5f, 0.55f);
        iconRt.pivot = new Vector2(0.5f, 0.5f);
        iconRt.anchoredPosition = Vector2.zero;
        iconRt.sizeDelta = new Vector2(64, 64);
        Image iconImg = iconObj.AddComponent<Image>();
        Sprite coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Tiny RPG Forest/Artwork/sprites/misc/coin/coin-1.png");
        if (coinSprite != null) iconImg.sprite = coinSprite;
        iconImg.preserveAspect = true;

        // 4. Progress Bar (Slider) container
        // Slider background
        GameObject sliderBgObj = CreateUIElement("ProgressBarBG", canvas.transform);
        RectTransform sliderBgRt = sliderBgObj.GetComponent<RectTransform>();
        sliderBgRt.anchorMin = new Vector2(0.5f, 0.35f);
        sliderBgRt.anchorMax = new Vector2(0.5f, 0.35f);
        sliderBgRt.pivot = new Vector2(0.5f, 0.5f);
        sliderBgRt.anchoredPosition = Vector2.zero;
        sliderBgRt.sizeDelta = new Vector2(600, 30);

        // Slider component
        Slider slider = sliderBgObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.interactable = false; // Người dùng không tương tác được

        // Slider Background Image
        Image sliderBgImage = sliderBgObj.AddComponent<Image>();
        sliderBgImage.color = new Color32(60, 50, 40, 200); // Nâu tối

        // Slider Fill Area 
        GameObject fillAreaObj = CreateUIElement("Fill Area", sliderBgObj.transform);
        RectTransform fillAreaRt = fillAreaObj.GetComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0, 0);
        fillAreaRt.anchorMax = new Vector2(1, 1);
        fillAreaRt.offsetMin = new Vector2(5, 5);
        fillAreaRt.offsetMax = new Vector2(-5, -5);

        // Slider Fill 
        GameObject fillObj = CreateUIElement("Fill", fillAreaObj.transform);
        RectTransform fillRt = fillObj.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = new Color32(120, 200, 80, 255); // Xanh lá tươi (nông trại)

        slider.fillRect = fillRt;

        // 5. Status Text
        GameObject statusObj = CreateUIElement("StatusText", canvas.transform);
        RectTransform statusRt = statusObj.GetComponent<RectTransform>();
        statusRt.anchorMin = new Vector2(0.5f, 0.25f);
        statusRt.anchorMax = new Vector2(0.5f, 0.25f);
        statusRt.pivot = new Vector2(0.5f, 0.5f);
        statusRt.anchoredPosition = Vector2.zero;
        statusRt.sizeDelta = new Vector2(700, 50);
        Text statusText = statusObj.AddComponent<Text>();
        statusText.text = "Đang kết nối máy chủ...";
        statusText.fontSize = 28;
        statusText.color = new Color32(200, 190, 170, 255); // Kem nhạt
        statusText.alignment = TextAnchor.MiddleCenter;
        statusText.horizontalOverflow = HorizontalWrapMode.Overflow;
        if (cherryFont != null) statusText.font = cherryFont;

        // 6. Fallback Button (ẩn mặc định)
        GameObject btnObj = CreateUIElement("ButtonLoadDone", canvas.transform);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.15f);
        btnRt.anchorMax = new Vector2(0.5f, 0.15f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.anchoredPosition = Vector2.zero;
        btnRt.sizeDelta = new Vector2(250, 60);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color32(90, 160, 60, 255); // Xanh lá
        Button btn = btnObj.AddComponent<Button>();
        btnObj.SetActive(false); // Ẩn mặc định

        // Button Text
        GameObject btnTextObj = CreateUIElement("Text", btnObj.transform);
        RectTransform btnTextRt = btnTextObj.GetComponent<RectTransform>();
        StretchFull(btnTextRt);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.text = "Tiếp tục";
        btnText.fontSize = 28;
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;
        if (cherryFont != null) btnText.font = cherryFont;

        // === GẮN LOADINGSCENE SCRIPT VÀO CANVAS ===
        LoadingScene loadingScene = canvas.gameObject.GetComponent<LoadingScene>();
        if (loadingScene == null)
            loadingScene = canvas.gameObject.AddComponent<LoadingScene>();

        loadingScene.progressBar = slider;
        loadingScene.statusText = statusText;
        loadingScene.buttonLoadDone = btn;

        // Đánh dấu Scene dirty để lưu được
        EditorUtility.SetDirty(canvas.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("SetupLoadingSceneUI: Hoàn tất tạo UI cho Loading Screen!");
    }

    private static GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
