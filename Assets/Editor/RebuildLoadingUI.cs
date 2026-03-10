using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Dọn sạch toàn bộ Canvas children rồi tạo lại UI Loading Screen từ đầu.
/// Kết hợp SetupLoadingSceneUI + UpgradeTitleFarm vào 1 script duy nhất.
/// </summary>
public class RebuildLoadingUI
{
    [MenuItem("Tools/RebuildLoadingUI")]
    public static void Apply()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Không tìm thấy Canvas!"); return; }

        // === XÓA HẾT CHILDREN CŨ ===
        while (canvas.transform.childCount > 0)
        {
            Object.DestroyImmediate(canvas.transform.GetChild(0).gameObject);
        }

        // === CẤU HÌNH CANVAS ===
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // === CẤU HÌNH CAMERA ===
        Camera cam = Camera.main ?? Object.FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(30, 25, 20, 255);
        }

        // Load font
        Font cherryFont = AssetDatabase.LoadAssetAtPath<Font>(
            "Assets/UI/Fonts/Cherry_Bomb_One/CherryBombOne-Regular.ttf");
        if (cherryFont == null)
            cherryFont = Font.CreateDynamicFontFromOSFont("Arial", 32);

        // Load sprites
        Sprite coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Tiny RPG Forest/Artwork/sprites/misc/coin/coin-1.png");
        Sprite treePink = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Tiny RPG Forest/Artwork/Environment/sliced-objects/tree-pink.png");
        Sprite treeOrange = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Tiny RPG Forest/Artwork/Environment/sliced-objects/tree-orange.png");

        // =====================================
        // 1. BACKGROUND PANEL (nền nâu tối)
        // =====================================
        GameObject bg = CreateUI("BackgroundPanel", canvas.transform);
        StretchFull(bg.GetComponent<RectTransform>());
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color32(35, 28, 22, 255);

        // =====================================
        // 2. CÂY TRANG TRÍ BÊN TRÁI
        // =====================================
        if (treePink != null)
        {
            GameObject treeL = CreateUI("TreeLeft", canvas.transform);
            SetupAnchoredUI(treeL, 0.12f, 0.68f, 160, 160);
            Image treeImg = treeL.AddComponent<Image>();
            treeImg.sprite = treePink;
            treeImg.preserveAspect = true;
            treeImg.color = new Color(1, 1, 1, 0.8f);
            treeL.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
        }

        // =====================================
        // 3. CÂY TRANG TRÍ BÊN PHẢI
        // =====================================
        if (treeOrange != null)
        {
            GameObject treeR = CreateUI("TreeRight", canvas.transform);
            SetupAnchoredUI(treeR, 0.88f, 0.68f, 160, 160);
            Image treeImgR = treeR.AddComponent<Image>();
            treeImgR.sprite = treeOrange;
            treeImgR.preserveAspect = true;
            treeImgR.color = new Color(1, 1, 1, 0.8f);
        }

        // =====================================
        // 4. TITLE TEXT "Moonlit Garden" (Legacy Text - ổn định)
        // =====================================
        GameObject titleObj = CreateUI("TitleText", canvas.transform);
        SetupAnchoredUI(titleObj, 0.5f, 0.72f, 800, 100);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "Moonlit Garden";
        titleText.font = cherryFont;
        titleText.fontSize = 72;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
        titleText.verticalOverflow = VerticalWrapMode.Overflow;
        titleText.color = new Color32(130, 210, 50, 255); // Xanh lá tươi

        // Viền outline xanh rêu
        Outline titleOutline = titleObj.AddComponent<Outline>();
        titleOutline.effectColor = new Color32(30, 60, 10, 255);
        titleOutline.effectDistance = new Vector2(3, -3);

        // Bóng đổ
        Shadow titleShadow = titleObj.AddComponent<Shadow>();
        titleShadow.effectColor = new Color32(10, 25, 5, 200);
        titleShadow.effectDistance = new Vector2(4, -5);

        // =====================================
        // 5. SUBTITLE "~ Khu vuon anh trang ~"
        // =====================================
        GameObject subObj = CreateUI("SubtitleText", canvas.transform);
        SetupAnchoredUI(subObj, 0.5f, 0.62f, 500, 40);
        Text subText = subObj.AddComponent<Text>();
        subText.text = "~ Khu vuon anh trang ~";
        subText.font = cherryFont;
        subText.fontSize = 24;
        subText.alignment = TextAnchor.MiddleCenter;
        subText.horizontalOverflow = HorizontalWrapMode.Overflow;
        subText.color = new Color32(220, 200, 160, 180);

        // =====================================
        // 6. LOADING ICON (coin)
        // =====================================
        GameObject iconObj = CreateUI("LoadingIcon", canvas.transform);
        SetupAnchoredUI(iconObj, 0.5f, 0.5f, 64, 64);
        Image iconImg = iconObj.AddComponent<Image>();
        if (coinSprite != null) iconImg.sprite = coinSprite;
        iconImg.preserveAspect = true;

        // =====================================
        // 7. PROGRESS BAR (Slider)
        // =====================================
        GameObject sliderBg = CreateUI("ProgressBarBG", canvas.transform);
        SetupAnchoredUI(sliderBg, 0.5f, 0.35f, 600, 24);
        Slider slider = sliderBg.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.interactable = false;
        Image sliderBgImg = sliderBg.AddComponent<Image>();
        sliderBgImg.color = new Color32(60, 50, 40, 200);

        // Fill Area
        GameObject fillArea = CreateUI("Fill Area", sliderBg.transform);
        RectTransform fillAreaRt = fillArea.GetComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.offsetMin = new Vector2(4, 4);
        fillAreaRt.offsetMax = new Vector2(-4, -4);

        // Fill
        GameObject fill = CreateUI("Fill", fillArea.transform);
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color32(120, 200, 80, 255); // Xanh lá tươi
        slider.fillRect = fillRt;

        // =====================================
        // 8. STATUS TEXT
        // =====================================
        GameObject statusObj = CreateUI("StatusText", canvas.transform);
        SetupAnchoredUI(statusObj, 0.5f, 0.25f, 700, 50);
        Text statusText = statusObj.AddComponent<Text>();
        statusText.text = "Dang ket noi may chu...";
        statusText.font = cherryFont;
        statusText.fontSize = 26;
        statusText.alignment = TextAnchor.MiddleCenter;
        statusText.horizontalOverflow = HorizontalWrapMode.Overflow;
        statusText.color = new Color32(200, 190, 170, 255);

        // =====================================
        // 9. FALLBACK BUTTON (ẩn mặc định)
        // =====================================
        GameObject btnObj = CreateUI("ButtonLoadDone", canvas.transform);
        SetupAnchoredUI(btnObj, 0.5f, 0.15f, 250, 60);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color32(90, 160, 60, 255);
        Button btn = btnObj.AddComponent<Button>();
        btnObj.SetActive(false);

        GameObject btnTextObj = CreateUI("Text", btnObj.transform);
        StretchFull(btnTextObj.GetComponent<RectTransform>());
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.text = "Tiep tuc";
        btnText.font = cherryFont;
        btnText.fontSize = 28;
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;

        // =====================================
        // GẮN LOADINGSCENE SCRIPT
        // =====================================
        LoadingScene loadingScene = canvas.gameObject.GetComponent<LoadingScene>();
        if (loadingScene == null)
            loadingScene = canvas.gameObject.AddComponent<LoadingScene>();
        loadingScene.progressBar = slider;
        loadingScene.statusText = statusText;
        loadingScene.buttonLoadDone = btn;

        EditorUtility.SetDirty(canvas.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("RebuildLoadingUI: Da tao lai toan bo UI Loading Screen thanh cong!");
    }

    private static GameObject CreateUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static void SetupAnchoredUI(GameObject go, float ax, float ay, float w, float h)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(ax, ay);
        rt.anchorMax = new Vector2(ax, ay);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(w, h);
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
