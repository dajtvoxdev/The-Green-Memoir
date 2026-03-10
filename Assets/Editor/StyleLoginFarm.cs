using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Thiết kế lại LoginScene phong cách nông trại.
/// Giữ nguyên cấu trúc form, chỉ thay đổi style: màu sắc, font, nền, và trang trí.
/// </summary>
public class StyleLoginFarm
{
    [MenuItem("Tools/StyleLoginFarm")]
    public static void Apply()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Không tìm thấy Canvas!"); return; }

        // Load resources
        Font cherryFont = AssetDatabase.LoadAssetAtPath<Font>(
            "Assets/UI/Fonts/Cherry_Bomb_One/CherryBombOne-Regular.ttf");
        if (cherryFont == null) cherryFont = Font.CreateDynamicFontFromOSFont("Arial", 32);

        Sprite treePink = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Tiny RPG Forest/Artwork/Environment/sliced-objects/tree-pink.png");
        Sprite treeOrange = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Tiny RPG Forest/Artwork/Environment/sliced-objects/tree-orange.png");
        Sprite bush = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Tiny RPG Forest/Artwork/Environment/sliced-objects/bush.png");
        Sprite bushTall = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Tiny RPG Forest/Artwork/Environment/sliced-objects/bush-tall.png");
        Sprite sign = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Tiny RPG Forest/Artwork/Environment/sliced-objects/sign.png");
        Sprite rock = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Tiny RPG Forest/Artwork/Environment/sliced-objects/rock.png");

        // === CẤU HÌNH CAMERA - nền đêm nông trại ===
        Camera cam = Camera.main ?? Object.FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(20, 30, 50, 255); // Xanh đêm tối
        }

        // === CẤU HÌNH CANVAS ===
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        Transform bgTr = canvas.transform.Find("Background");
        if (bgTr == null) { Debug.LogError("Không tìm thấy Background!"); return; }

        // === BACKGROUND - gradient xanh đêm → nâu đất ===
        Image bgImage = bgTr.GetComponent<Image>();
        if (bgImage != null)
        {
            bgImage.color = new Color32(25, 35, 55, 255); // Xanh đêm pha tím
        }

        // Xóa các trang trí cũ nếu chạy lại
        string[] decoNames = { "FarmTreeLeft", "FarmTreeRight", "FarmBushLeft", "FarmBushRight",
                               "FarmGround", "FarmSign", "FarmRock", "FarmBushTallLeft",
                               "FarmBushTallRight", "MoonGlow", "Stars" };
        foreach (string n in decoNames)
        {
            Transform old = canvas.transform.Find(n);
            if (old != null) Object.DestroyImmediate(old.gameObject);
        }

        // === THÊM MẶT ĐẤT (dải nâu bên dưới) ===
        GameObject ground = CreateUI("FarmGround", canvas.transform);
        RectTransform groundRt = ground.GetComponent<RectTransform>();
        groundRt.anchorMin = new Vector2(0, 0);
        groundRt.anchorMax = new Vector2(1, 0.15f);
        groundRt.offsetMin = Vector2.zero;
        groundRt.offsetMax = Vector2.zero;
        Image groundImg = ground.AddComponent<Image>();
        groundImg.color = new Color32(60, 45, 30, 255); // Nâu đất

        // === TRĂNG SÁNG (trên góc phải) ===
        GameObject moon = CreateUI("MoonGlow", canvas.transform);
        SetupAnchor(moon, 0.85f, 0.85f, 120, 120);
        Image moonImg = moon.AddComponent<Image>();
        moonImg.color = new Color32(255, 240, 180, 100); // Vàng kem mờ
        // Bo tròn bằng cách dùng default sprite

        // === NGÔI SAO (vài chấm sáng trên nền đêm) ===
        float[][] starPositions = new float[][] {
            new float[]{0.1f, 0.9f}, new float[]{0.25f, 0.85f}, new float[]{0.4f, 0.92f},
            new float[]{0.6f, 0.88f}, new float[]{0.75f, 0.95f}, new float[]{0.15f, 0.82f},
            new float[]{0.5f, 0.95f}, new float[]{0.9f, 0.78f}, new float[]{0.35f, 0.8f},
            new float[]{0.65f, 0.93f}, new float[]{0.8f, 0.82f}, new float[]{0.05f, 0.95f}
        };
        GameObject starsParent = CreateUI("Stars", canvas.transform);
        RectTransform starsRt = starsParent.GetComponent<RectTransform>();
        starsRt.anchorMin = Vector2.zero;
        starsRt.anchorMax = Vector2.one;
        starsRt.offsetMin = Vector2.zero;
        starsRt.offsetMax = Vector2.zero;
        foreach (var pos in starPositions)
        {
            GameObject star = CreateUI("Star", starsParent.transform);
            float size = Random.Range(3f, 8f);
            SetupAnchor(star, pos[0], pos[1], size, size);
            Image starImg = star.AddComponent<Image>();
            int brightness = Random.Range(180, 255);
            starImg.color = new Color32((byte)brightness, (byte)brightness, (byte)(brightness - 20), (byte)Random.Range(120, 220));
        }

        // === CÂY TRANG TRÍ ===
        if (treePink != null)
            CreateDecoration("FarmTreeLeft", canvas.transform, treePink, 0.05f, 0.22f, 200, 200, true);
        if (treeOrange != null)
            CreateDecoration("FarmTreeRight", canvas.transform, treeOrange, 0.95f, 0.22f, 200, 200, false);

        // === BỤI CÂY ===
        if (bush != null)
        {
            CreateDecoration("FarmBushLeft", canvas.transform, bush, 0.15f, 0.13f, 100, 80, false);
            CreateDecoration("FarmBushRight", canvas.transform, bush, 0.85f, 0.13f, 100, 80, true);
        }
        if (bushTall != null)
        {
            CreateDecoration("FarmBushTallLeft", canvas.transform, bushTall, 0.22f, 0.2f, 80, 100, false);
            CreateDecoration("FarmBushTallRight", canvas.transform, bushTall, 0.78f, 0.2f, 80, 100, true);
        }

        // === BIỂN GỖ (dưới cùng) ===
        if (sign != null)
            CreateDecoration("FarmSign", canvas.transform, sign, 0.12f, 0.18f, 90, 90, false);
        if (rock != null)
            CreateDecoration("FarmRock", canvas.transform, rock, 0.9f, 0.13f, 70, 60, false);

        // === STYLE CÁC FORM ===
        Transform loginForm = bgTr.Find("LoginForm");
        if (loginForm != null)
        {
            // LoginForm background - bảng gỗ nâu bán trong suốt
            Image formImg = loginForm.GetComponent<Image>();
            if (formImg != null)
                formImg.color = new Color32(50, 35, 25, 200);

            // Header
            StyleHeader(loginForm.Find("Header"), cherryFont);

            // LoginWithAccount panel
            StyleFormPanel(loginForm.Find("LoginWithAccount"), cherryFont, "Dang nhap");

            // RegisterForm panel
            StyleFormPanel(loginForm.Find("RegisterForm"), cherryFont, "Dang ky");

            // LoginFast panel
            StyleLoginFastPanel(loginForm.Find("LoginFast"), cherryFont);
        }

        // Đánh dấu dirty
        EditorUtility.SetDirty(canvas.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("StyleLoginFarm: LoginScene da duoc thiet ke lai phong cach nong trai!");
    }

    private static void StyleHeader(Transform header, Font font)
    {
        if (header == null) return;

        // Header background - trong suốt, không cần nền
        Image headerImg = header.GetComponent<Image>();
        if (headerImg != null)
            headerImg.color = new Color(0, 0, 0, 0);

        // GameTitle
        Transform gameTitle = header.Find("GameTitle");
        if (gameTitle != null)
        {
            Text titleText = gameTitle.GetComponent<Text>();
            if (titleText != null)
            {
                titleText.text = "Moonlit Garden";
                titleText.font = font;
                titleText.fontSize = 64;
                titleText.fontStyle = FontStyle.Bold;
                titleText.color = new Color32(130, 210, 50, 255); // Xanh lá tươi
                titleText.alignment = TextAnchor.MiddleCenter;
                titleText.horizontalOverflow = HorizontalWrapMode.Overflow;

                // Thêm outline nếu chưa có
                Outline outline = gameTitle.GetComponent<Outline>();
                if (outline == null) outline = gameTitle.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color32(30, 60, 10, 255);
                outline.effectDistance = new Vector2(3, -3);

                Shadow shadow = gameTitle.GetComponent<Shadow>();
                if (shadow == null) shadow = gameTitle.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color32(10, 25, 5, 200);
                shadow.effectDistance = new Vector2(4, -5);
            }
        }
    }

    private static void StyleFormPanel(Transform panel, Font font, string titleLabel)
    {
        if (panel == null) return;

        // Panel background - bảng gỗ nâu ấm
        Image panelImg = panel.GetComponent<Image>();
        if (panelImg != null)
            panelImg.color = new Color32(60, 42, 28, 220);

        // Style tất cả Text children
        foreach (Text text in panel.GetComponentsInChildren<Text>(true))
        {
            text.font = font;
            // Text label (Login/Register title)
            if (text.gameObject.name == "Login" || text.gameObject.name == "Register")
            {
                text.fontSize = 36;
                text.color = new Color32(255, 220, 120, 255); // Vàng ấm
                text.fontStyle = FontStyle.Bold;
            }
            // Notify text
            else if (text.gameObject.name.Contains("Notify"))
            {
                text.fontSize = 18;
                text.color = new Color32(255, 100, 80, 255); // Đỏ cam nhẹ
            }
            // Forgot password
            else if (text.gameObject.name == "ForgotPassword")
            {
                text.fontSize = 18;
                text.color = new Color32(180, 220, 255, 200); // Xanh dương nhạt
            }
            // Button text
            else if (text.transform.parent != null && text.transform.parent.GetComponent<Button>() != null)
            {
                text.fontSize = 22;
                text.fontStyle = FontStyle.Bold;
                text.color = Color.white;
            }
            // Placeholder text
            else if (text.gameObject.name == "Placeholder")
            {
                text.fontSize = 20;
                text.color = new Color32(160, 140, 120, 150); // Nâu nhạt mờ
                text.fontStyle = FontStyle.Italic;
            }
            // Input text
            else if (text.gameObject.name == "Text")
            {
                text.fontSize = 22;
                text.color = new Color32(240, 230, 210, 255); // Kem sáng
            }
        }

        // Style InputFields
        foreach (InputField input in panel.GetComponentsInChildren<InputField>(true))
        {
            Image inputImg = input.GetComponent<Image>();
            if (inputImg != null)
                inputImg.color = new Color32(40, 30, 22, 200); // Nâu sẫm

            Outline inputOutline = input.GetComponent<Outline>();
            if (inputOutline != null)
                inputOutline.effectColor = new Color32(100, 80, 50, 180); // Nâu viền
        }

        // Style Buttons
        foreach (Button btn in panel.GetComponentsInChildren<Button>(true))
        {
            Image btnImg = btn.GetComponent<Image>();
            if (btnImg != null)
            {
                if (btn.gameObject.name.Contains("Login") || btn.gameObject.name.Contains("Register"))
                {
                    // Nút chính - xanh lá nông trại
                    btnImg.color = new Color32(80, 150, 50, 255);
                }
                else
                {
                    // Nút phụ - nâu gỗ
                    btnImg.color = new Color32(100, 70, 45, 220);
                }
            }
        }

        // Style separator lines
        Transform line = panel.Find("LeftLine");
        if (line != null)
        {
            Image lineImg = line.GetComponent<Image>();
            if (lineImg != null)
                lineImg.color = new Color32(100, 80, 50, 120);
        }
    }

    private static void StyleLoginFastPanel(Transform panel, Font font)
    {
        if (panel == null) return;

        // Panel background - trong suốt hơn
        Image panelImg = panel.GetComponent<Image>();
        if (panelImg != null)
            panelImg.color = new Color32(50, 35, 25, 180);

        // Style all texts
        foreach (Text text in panel.GetComponentsInChildren<Text>(true))
        {
            text.font = font;
            if (text.gameObject.name == "OrLoginWith")
            {
                text.fontSize = 22;
                text.color = new Color32(200, 190, 170, 200);
            }
            else
            {
                text.fontSize = 20;
                text.color = Color.white;
                text.fontStyle = FontStyle.Bold;
            }
        }

        // Style buttons
        foreach (Button btn in panel.GetComponentsInChildren<Button>(true))
        {
            Image btnImg = btn.GetComponent<Image>();
            if (btnImg != null)
            {
                if (btn.gameObject.name.Contains("Facebook"))
                    btnImg.color = new Color32(60, 90, 150, 230); // Xanh Facebook
                else if (btn.gameObject.name.Contains("Google"))
                    btnImg.color = new Color32(180, 60, 40, 230); // Đỏ Google
                else if (btn.gameObject.name.Contains("PlayNow"))
                    btnImg.color = new Color32(80, 150, 50, 255); // Xanh lá
            }

            Outline outline = btn.GetComponent<Outline>();
            if (outline != null)
                outline.effectColor = new Color32(30, 20, 10, 180);
        }

        // Separator line
        Transform line = panel.Find("RightLine");
        if (line != null)
        {
            Image lineImg = line.GetComponent<Image>();
            if (lineImg != null)
                lineImg.color = new Color32(100, 80, 50, 120);
        }
    }

    // === HELPER FUNCTIONS ===
    private static GameObject CreateUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static void SetupAnchor(GameObject go, float ax, float ay, float w, float h)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(ax, ay);
        rt.anchorMax = new Vector2(ax, ay);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(w, h);
    }

    private static void CreateDecoration(string name, Transform parent, Sprite sprite,
        float ax, float ay, float w, float h, bool flipX)
    {
        GameObject obj = CreateUI(name, parent);
        SetupAnchor(obj, ax, ay, w, h);
        Image img = obj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.color = new Color(1, 1, 1, 0.85f);
        if (flipX) obj.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
    }
}
