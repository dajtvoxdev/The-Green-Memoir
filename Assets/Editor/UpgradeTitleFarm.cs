using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Nâng cấp Title Loading Screen phong cách nông trại.
/// Dùng Legacy Text (ổn định khi tạo từ Editor Script) + Cherry Bomb font + Outline + Shadow.
/// </summary>
public class UpgradeTitleFarm
{
    [MenuItem("Tools/UpgradeTitleFarm")]
    public static void Apply()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Không tìm thấy Canvas!"); return; }

        Font cherryFont = AssetDatabase.LoadAssetAtPath<Font>(
            "Assets/UI/Fonts/Cherry_Bomb_One/CherryBombOne-Regular.ttf");
        if (cherryFont == null)
            cherryFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // --- XÓA ELEMENTS CŨ ---
        string[] oldNames = { "TitleText", "SubtitleText", "TreeLeft", "TreeRight", "LeafLeft", "LeafRight" };
        foreach (string n in oldNames)
        {
            Transform old = canvas.transform.Find(n);
            if (old != null) Object.DestroyImmediate(old.gameObject);
        }

        // ===== TITLE "Moonlit Garden" =====
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(canvas.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>() ?? titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.72f);
        titleRt.anchorMax = new Vector2(0.5f, 0.72f);
        titleRt.pivot = new Vector2(0.5f, 0.5f);
        titleRt.anchoredPosition = Vector2.zero;
        titleRt.sizeDelta = new Vector2(800, 100);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "Moonlit Garden";
        titleText.font = cherryFont;
        titleText.fontSize = 72;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
        titleText.verticalOverflow = VerticalWrapMode.Overflow;
        // Màu xanh lá sáng tươi - tone nông trại
        titleText.color = new Color32(130, 210, 50, 255);

        // Viền xanh rêu đậm
        Outline titleOutline = titleObj.AddComponent<Outline>();
        titleOutline.effectColor = new Color32(30, 60, 10, 255);
        titleOutline.effectDistance = new Vector2(3, -3);

        // Bóng đổ
        Shadow titleShadow = titleObj.AddComponent<Shadow>();
        titleShadow.effectColor = new Color32(10, 25, 5, 200);
        titleShadow.effectDistance = new Vector2(4, -5);

        // ===== SUBTITLE =====
        GameObject subObj = new GameObject("SubtitleText");
        subObj.transform.SetParent(canvas.transform, false);
        RectTransform subRt = subObj.GetComponent<RectTransform>() ?? subObj.AddComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0.5f, 0.62f);
        subRt.anchorMax = new Vector2(0.5f, 0.62f);
        subRt.pivot = new Vector2(0.5f, 0.5f);
        subRt.anchoredPosition = Vector2.zero;
        subRt.sizeDelta = new Vector2(500, 40);

        Text subText = subObj.AddComponent<Text>();
        subText.text = "~ Khu vuon anh trang ~";
        subText.font = cherryFont;
        subText.fontSize = 24;
        subText.alignment = TextAnchor.MiddleCenter;
        subText.horizontalOverflow = HorizontalWrapMode.Overflow;
        subText.color = new Color32(220, 200, 160, 180);

        // ===== CÂY TRANG TRÍ 2 BÊN =====
        Sprite treePink = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Tiny RPG Forest/Artwork/Environment/sliced-objects/tree-pink.png");
        Sprite treeOrange = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Tiny RPG Forest/Artwork/Environment/sliced-objects/tree-orange.png");

        if (treePink != null)
            CreateDecoImage("TreeLeft", canvas.transform, treePink,
                new Vector2(0.12f, 0.68f), new Vector2(160, 160), true);

        if (treeOrange != null)
            CreateDecoImage("TreeRight", canvas.transform, treeOrange,
                new Vector2(0.88f, 0.68f), new Vector2(160, 160), false);

        EditorUtility.SetDirty(canvas.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("UpgradeTitleFarm: Title nong trai da duoc ap dung thanh cong!");
    }

    private static void CreateDecoImage(string name, Transform parent, Sprite sprite,
        Vector2 anchor, Vector2 size, bool flipX)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>() ?? obj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.color = new Color(1, 1, 1, 0.8f);

        if (flipX) rt.localScale = new Vector3(-1, 1, 1);
    }
}
