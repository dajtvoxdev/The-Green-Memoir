using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Xóa sạch Canvas cũ của LoginScene rồi tạo lại toàn bộ UI phong cách nông trại.
/// Tên game: The Green Memoir. Giữ nguyên chức năng Firebase Login/Register.
/// </summary>
public class RebuildLoginUI
{
    // Màu palette nông trại đêm
    static readonly Color32 COL_NIGHT     = new Color32(20, 30, 55, 255);
    static readonly Color32 COL_GROUND    = new Color32(55, 40, 28, 255);
    static readonly Color32 COL_WOOD_DARK = new Color32(65, 45, 30, 240);
    static readonly Color32 COL_WOOD_MED  = new Color32(85, 60, 40, 230);
    static readonly Color32 COL_INPUT_BG  = new Color32(35, 25, 18, 220);
    static readonly Color32 COL_INPUT_BD  = new Color32(110, 85, 55, 200);
    static readonly Color32 COL_BTN_GREEN = new Color32(75, 145, 45, 255);
    static readonly Color32 COL_TITLE     = new Color32(130, 210, 50, 255);
    static readonly Color32 COL_SUBTITLE  = new Color32(220, 200, 160, 180);
    static readonly Color32 COL_LABEL     = new Color32(255, 220, 120, 255);
    static readonly Color32 COL_TEXT      = new Color32(235, 225, 205, 255);
    static readonly Color32 COL_HINT      = new Color32(150, 130, 110, 150);
    static readonly Color32 COL_LINK      = new Color32(170, 210, 255, 200);
    static readonly Color32 COL_MOON      = new Color32(255, 245, 200, 80);

    static Font titleFont;  // Cherry Bomb - cho title tiếng Anh
    static Font vietFont;   // Arial - cho label tiếng Việt có dấu

    [MenuItem("Tools/RebuildLoginUI")]
    public static void Apply()
    {
        // === LOAD FONTS ===
        titleFont = AssetDatabase.LoadAssetAtPath<Font>(
            "Assets/UI/Fonts/Cherry_Bomb_One/CherryBombOne-Regular.ttf");
        vietFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (titleFont == null) titleFont = vietFont;

        // === LOAD SPRITES ===
        Sprite logo = LoadSprite("Assets/Image/04306d64-faac-4114-b77b-ba300e092e13.png");
        Sprite treePink = LoadSprite("Assets/Tiny RPG Forest/Artwork/Environment/sliced-objects/tree-pink.png");
        Sprite treeOrange = LoadSprite("Assets/Tiny RPG Forest/Artwork/Environment/sliced-objects/tree-orange.png");
        Sprite bush = LoadSprite("Assets/Tiny RPG Forest/Artwork/Environment/sliced-objects/bush.png");
        Sprite bushTall = LoadSprite("Assets/Tiny RPG Forest/Artwork/Environment/sliced-objects/bush-tall.png");
        Sprite sign = LoadSprite("Assets/Tiny RPG Forest/Artwork/Environment/sliced-objects/sign.png");
        Sprite rock = LoadSprite("Assets/Tiny RPG Forest/Artwork/Environment/sliced-objects/rock.png");

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Khong tim thay Canvas!"); return; }

        // === XÓA SẠCH CHILDREN CŨ ===
        while (canvas.transform.childCount > 0)
            Object.DestroyImmediate(canvas.transform.GetChild(0).gameObject);

        // === CẤU HÌNH CANVAS ===
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // === CAMERA ===
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(15, 20, 40, 255);
        }

        // =============================================
        //  LỚP 1: NỀN ĐÊM + MẶT ĐẤT
        // =============================================
        MakeStretchPanel("Background", canvas.transform, COL_NIGHT);

        // Mặt đất nâu
        GameObject ground = MakeUI("Ground", canvas.transform);
        RectTransform gRt = ground.GetComponent<RectTransform>();
        gRt.anchorMin = new Vector2(0, 0);
        gRt.anchorMax = new Vector2(1, 0.18f);
        gRt.offsetMin = gRt.offsetMax = Vector2.zero;
        ground.AddComponent<Image>().color = COL_GROUND;

        // =============================================
        //  LỚP 2: TRĂNG + SAO
        // =============================================
        MakeDecoRect("MoonGlow", canvas.transform, 0.82f, 0.82f, 140, 140, COL_MOON);

        float[][] stars = {
            new[]{0.08f, 0.92f}, new[]{0.2f, 0.88f}, new[]{0.32f, 0.94f},
            new[]{0.48f, 0.9f},  new[]{0.6f, 0.95f}, new[]{0.72f, 0.87f},
            new[]{0.88f, 0.93f}, new[]{0.15f, 0.82f}, new[]{0.55f, 0.85f},
            new[]{0.78f, 0.91f}, new[]{0.42f, 0.83f}, new[]{0.95f, 0.88f}
        };
        foreach (var s in stars)
        {
            float sz = Random.Range(3f, 7f);
            byte b = (byte)Random.Range(200, 255);
            MakeDecoRect("Star", canvas.transform, s[0], s[1], sz, sz,
                new Color32(b, b, (byte)(b - 15), (byte)Random.Range(140, 230)));
        }

        // =============================================
        //  LỚP 3: CÂY CỐI BỤI ĐÁ TRANG TRÍ
        // =============================================
        if (treePink != null)   MakeDecoSprite("TreeLeft",  canvas.transform, treePink,  0.04f, 0.26f, 220, 220, true);
        if (treeOrange != null) MakeDecoSprite("TreeRight", canvas.transform, treeOrange, 0.96f, 0.26f, 220, 220, false);
        if (bush != null)
        {
            MakeDecoSprite("BushL", canvas.transform, bush, 0.14f, 0.15f, 100, 80, false);
            MakeDecoSprite("BushR", canvas.transform, bush, 0.86f, 0.15f, 100, 80, true);
        }
        if (bushTall != null)
        {
            MakeDecoSprite("BushTallL", canvas.transform, bushTall, 0.21f, 0.23f, 80, 110, false);
            MakeDecoSprite("BushTallR", canvas.transform, bushTall, 0.79f, 0.23f, 80, 110, true);
        }
        if (sign != null) MakeDecoSprite("Sign", canvas.transform, sign, 0.11f, 0.2f, 90, 90, false);
        if (rock != null) MakeDecoSprite("Rock", canvas.transform, rock, 0.91f, 0.16f, 65, 55, false);

        // =============================================
        //  LỚP 4: LOGO + TITLE "The Green Memoir"
        // =============================================
        // Logo game
        if (logo != null)
        {
            GameObject logoObj = MakeUI("GameLogo", canvas.transform);
            SetAnchor(logoObj, 0.5f, 0.88f, 180, 180);
            Image logoImg = logoObj.AddComponent<Image>();
            logoImg.sprite = logo;
            logoImg.preserveAspect = true;
        }

        // Title text "The Green Memoir" (tiếng Anh → dùng Cherry Bomb)
        GameObject titleObj = MakeUI("GameTitle", canvas.transform);
        SetAnchor(titleObj, 0.5f, 0.78f, 600, 55);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "The Green Memoir";
        titleText.font = titleFont;
        titleText.fontSize = 42;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
        titleText.verticalOverflow = VerticalWrapMode.Overflow;
        titleText.color = COL_TITLE;
        AddOutline(titleObj, new Color32(25, 55, 10, 255), 3, -3);
        AddShadow(titleObj, new Color32(10, 20, 5, 200), 3, -4);

        // =============================================
        //  LỚP 5: FORM CONTAINER (bảng gỗ trung tâm)
        // =============================================
        GameObject formContainer = MakeUI("FormContainer", canvas.transform);
        RectTransform fcRt = formContainer.GetComponent<RectTransform>();
        fcRt.anchorMin = new Vector2(0.15f, 0.2f);
        fcRt.anchorMax = new Vector2(0.85f, 0.72f);
        fcRt.offsetMin = fcRt.offsetMax = Vector2.zero;
        Image fcImg = formContainer.AddComponent<Image>();
        fcImg.color = COL_WOOD_DARK;
        AddOutline(formContainer, new Color32(40, 28, 18, 200), 4, -4);

        // =============================================
        //  LỚP 6A: LOGIN FORM (bên phải)
        // =============================================
        GameObject loginPanel = MakeUI("LoginPanel", formContainer.transform);
        RectTransform lpRt = loginPanel.GetComponent<RectTransform>();
        lpRt.anchorMin = new Vector2(0.52f, 0.05f);
        lpRt.anchorMax = new Vector2(0.98f, 0.95f);
        lpRt.offsetMin = lpRt.offsetMax = Vector2.zero;
        loginPanel.AddComponent<Image>().color = COL_WOOD_MED;

        // Tiêu đề (tiếng Việt có dấu → dùng Arial/vietFont)
        MakeLabel("LoginTitle", loginPanel.transform, 0.5f, 0.88f, 300, 45,
            "Đăng nhập", 34, COL_LABEL, vietFont);

        // InputField Email
        InputField ipLoginEmail = MakeInputField("IpLoginEmail", loginPanel.transform,
            0.5f, 0.68f, 360, 48, "Email...");

        // InputField Password
        InputField ipLoginPassword = MakeInputField("IpLoginPassword", loginPanel.transform,
            0.5f, 0.50f, 360, 48, "Mật khẩu...");
        ipLoginPassword.contentType = InputField.ContentType.Password;

        // Nút "ĐĂNG NHẬP" (xanh lá)
        Button btnLogin = MakeButton("ButtonLogin", loginPanel.transform,
            0.5f, 0.30f, 220, 50, "ĐĂNG NHẬP", COL_BTN_GREEN, Color.white, 22);

        // Link "Chưa có tài khoản? Đăng ký ngay"
        Button btnMoveToRegister = MakeButton("ButtonMoveToRegister", loginPanel.transform,
            0.5f, 0.14f, 380, 35, "Chưa có tài khoản? Đăng ký ngay",
            new Color(0,0,0,0), COL_LINK, 17, true);

        // =============================================
        //  LỚP 6B: REGISTER FORM (cùng vị trí, ẩn mặc định)
        // =============================================
        GameObject registerPanel = MakeUI("RegisterPanel", formContainer.transform);
        RectTransform rpRt = registerPanel.GetComponent<RectTransform>();
        rpRt.anchorMin = new Vector2(0.52f, 0.05f);
        rpRt.anchorMax = new Vector2(0.98f, 0.95f);
        rpRt.offsetMin = rpRt.offsetMax = Vector2.zero;
        registerPanel.AddComponent<Image>().color = COL_WOOD_MED;
        registerPanel.SetActive(false);

        MakeLabel("RegisterTitle", registerPanel.transform, 0.5f, 0.90f, 300, 45,
            "Đăng ký", 34, COL_LABEL, vietFont);

        InputField ipRegEmail = MakeInputField("IpRegisterEmail", registerPanel.transform,
            0.5f, 0.72f, 360, 48, "Email...");

        InputField ipRegPassword = MakeInputField("IpRegisterPassword", registerPanel.transform,
            0.5f, 0.54f, 360, 48, "Mật khẩu...");
        ipRegPassword.contentType = InputField.ContentType.Password;

        Button btnRegister = MakeButton("ButtonRegister", registerPanel.transform,
            0.5f, 0.35f, 220, 50, "ĐĂNG KÝ", COL_BTN_GREEN, Color.white, 22);

        Button btnMoveToSignIn = MakeButton("ButtonMoveToSignIn", registerPanel.transform,
            0.5f, 0.18f, 380, 35, "Đã có tài khoản? Đăng nhập",
            new Color(0,0,0,0), COL_LINK, 17, true);

        // =============================================
        //  LỚP 6C: LOGIN FAST (bên trái)
        // =============================================
        GameObject fastPanel = MakeUI("LoginFastPanel", formContainer.transform);
        RectTransform fpRt = fastPanel.GetComponent<RectTransform>();
        fpRt.anchorMin = new Vector2(0.02f, 0.05f);
        fpRt.anchorMax = new Vector2(0.48f, 0.95f);
        fpRt.offsetMin = fpRt.offsetMax = Vector2.zero;
        fastPanel.AddComponent<Image>().color = COL_WOOD_MED;

        MakeLabel("OrLoginWith", fastPanel.transform, 0.5f, 0.88f, 380, 35,
            "Hoặc đăng nhập bằng", 22, COL_SUBTITLE, vietFont);

        // Đường kẻ phân cách
        MakeDecoRect("SepLine", fastPanel.transform, 0.5f, 0.80f, 320, 2,
            new Color32(100, 80, 55, 150));

        // Google (đỏ Google)
        Button btnLoginGoogle = MakeButton("LoginWithGoogle", fastPanel.transform,
            0.5f, 0.60f, 340, 55, "GOOGLE",
            new Color32(175, 55, 35, 255), Color.white, 22);

        // Chơi ngay (Guest / Anonymous)
        Button btnPlayNow = MakeButton("PlayNow", fastPanel.transform,
            0.5f, 0.35f, 340, 55, "CHƠI NGAY",
            COL_BTN_GREEN, Color.white, 24);

        // =============================================
        //  WIRE FIREBASE LOGIN MANAGER
        // =============================================
        GameObject authGO = GameObject.Find("FireBaseAuthManager");
        if (authGO != null)
        {
            FirebaseLoginManager mgr = authGO.GetComponent<FirebaseLoginManager>();
            if (mgr != null)
            {
                mgr.ipLoginEmail = ipLoginEmail;
                mgr.ipLoginPassword = ipLoginPassword;
                mgr.buttonLogin = btnLogin;
                mgr.ipRegisterEmail = ipRegEmail;
                mgr.ipRegisterPassword = ipRegPassword;
                mgr.buttonRegister = btnRegister;
                mgr.buttonMoveToSignIn = btnMoveToSignIn;
                mgr.buttonMoveToRegister = btnMoveToRegister;
                mgr.loginForm = loginPanel;
                mgr.registerForm = registerPanel;
                // Wire nút đăng nhập nhanh
                mgr.buttonLoginGoogle = btnLoginGoogle;
                mgr.buttonPlayNow = btnPlayNow;
                EditorUtility.SetDirty(mgr);
                Debug.Log("Wire FirebaseLoginManager: Thanh cong 12 tham chieu!");
            }
        }
        else
        {
            Debug.LogWarning("Khong tim thay FireBaseAuthManager!");
        }

        EditorUtility.SetDirty(canvas.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("RebuildLoginUI: Xay lai LoginScene 'The Green Memoir' thanh cong!");
    }

    // ================================================
    //  HELPER FUNCTIONS
    // ================================================

    static Sprite LoadSprite(string path) =>
        AssetDatabase.LoadAssetAtPath<Sprite>(path);

    static GameObject MakeUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static void MakeStretchPanel(string name, Transform parent, Color32 color)
    {
        GameObject go = MakeUI(name, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = color;
    }

    static void SetAnchor(GameObject go, float ax, float ay, float w, float h)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(ax, ay);
        rt.anchorMax = new Vector2(ax, ay);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(w, h);
    }

    static void MakeDecoRect(string name, Transform parent, float ax, float ay,
        float w, float h, Color32 c)
    {
        GameObject go = MakeUI(name, parent);
        SetAnchor(go, ax, ay, w, h);
        go.AddComponent<Image>().color = c;
    }

    static void MakeDecoSprite(string name, Transform parent, Sprite sp,
        float ax, float ay, float w, float h, bool flipX)
    {
        GameObject go = MakeUI(name, parent);
        SetAnchor(go, ax, ay, w, h);
        Image img = go.AddComponent<Image>();
        img.sprite = sp;
        img.preserveAspect = true;
        img.color = new Color(1, 1, 1, 0.85f);
        if (flipX) go.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
    }

    static Text MakeLabel(string name, Transform parent, float ax, float ay,
        float w, float h, string text, int fontSize, Color32 color, Font font)
    {
        GameObject go = MakeUI(name, parent);
        SetAnchor(go, ax, ay, w, h);
        Text t = go.AddComponent<Text>();
        t.text = text;
        t.font = font;
        t.fontSize = fontSize;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.color = color;
        return t;
    }

    static InputField MakeInputField(string name, Transform parent,
        float ax, float ay, float w, float h, string placeholder)
    {
        GameObject go = MakeUI(name, parent);
        SetAnchor(go, ax, ay, w, h);

        Image bg = go.AddComponent<Image>();
        bg.color = COL_INPUT_BG;

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = COL_INPUT_BD;
        outline.effectDistance = new Vector2(2, -2);

        // Text hiển thị (dùng vietFont cho tiếng Việt)
        GameObject textGO = MakeUI("Text", go.transform);
        StretchChild(textGO, 12, 4);
        Text textComp = textGO.AddComponent<Text>();
        textComp.font = vietFont;
        textComp.fontSize = 22;
        textComp.color = COL_TEXT;
        textComp.alignment = TextAnchor.MiddleLeft;
        textComp.supportRichText = false;

        // Placeholder
        GameObject phGO = MakeUI("Placeholder", go.transform);
        StretchChild(phGO, 12, 4);
        Text phText = phGO.AddComponent<Text>();
        phText.text = placeholder;
        phText.font = vietFont;
        phText.fontSize = 22;
        phText.fontStyle = FontStyle.Italic;
        phText.color = COL_HINT;
        phText.alignment = TextAnchor.MiddleLeft;

        InputField input = go.AddComponent<InputField>();
        input.textComponent = textComp;
        input.placeholder = phText;
        input.targetGraphic = bg;

        return input;
    }

    // overload: isViet = false → dùng titleFont, true → dùng vietFont
    static Button MakeButton(string name, Transform parent,
        float ax, float ay, float w, float h,
        string label, Color32 bgColor, Color textColor, int fontSize,
        bool useVietFont = false)
    {
        GameObject go = MakeUI(name, parent);
        SetAnchor(go, ax, ay, w, h);

        Image img = go.AddComponent<Image>();
        img.color = bgColor;

        Button btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1, 1, 1, 0.85f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = colors;
        btn.targetGraphic = img;

        GameObject textGO = MakeUI("Text", go.transform);
        StretchChild(textGO, 0, 0);
        Text t = textGO.AddComponent<Text>();
        t.text = label;
        t.font = useVietFont ? vietFont : titleFont;
        t.fontSize = fontSize;
        t.fontStyle = FontStyle.Bold;
        t.color = textColor;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;

        return btn;
    }

    static void StretchChild(GameObject go, float padX, float padY)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padX, padY);
        rt.offsetMax = new Vector2(-padX, -padY);
    }

    static void AddOutline(GameObject go, Color32 c, float dx, float dy)
    {
        Outline o = go.GetComponent<Outline>() ?? go.AddComponent<Outline>();
        o.effectColor = c;
        o.effectDistance = new Vector2(dx, dy);
    }

    static void AddShadow(GameObject go, Color32 c, float dx, float dy)
    {
        Shadow s = go.GetComponent<Shadow>() ?? go.AddComponent<Shadow>();
        s.effectColor = c;
        s.effectDistance = new Vector2(dx, dy);
    }
}
