using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ApplyFarmUI_Phase5
{
    [MenuItem("Tools/ApplyFarmUI_Phase5")]
    public static void Apply()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Cleanup problematic components
        foreach (var lg in canvas.GetComponentsInChildren<LayoutGroup>(true))
        {
            if (lg.gameObject.name == "EconomyHUD" || lg.gameObject.name == "TimeHUD" || lg.gameObject.name == "Gold" || lg.gameObject.name == "Diamond")
                Object.DestroyImmediate(lg);
        }
        foreach (var csf in canvas.GetComponentsInChildren<ContentSizeFitter>(true))
        {
            if (csf.gameObject.name == "EconomyHUD" || csf.gameObject.name == "TimeHUD" || csf.gameObject.name == "Gold" || csf.gameObject.name == "Diamond")
                Object.DestroyImmediate(csf);
        }

        Transform eHud = null;
        Transform tHud = null;
        foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "EconomyHUD") eHud = t;
            if (t.name == "TimeHUD") tHud = t;
        }

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/Cherry_Bomb_One/CherryBombOne-Regular SDF.asset");

        if (eHud != null)
        {
            var rt = eHud.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1); rt.anchorMax = new Vector2(1, 1); rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-40, -40); rt.sizeDelta = new Vector2(300, 60);

            var goldObj = eHud.Find("Gold");
            if (goldObj != null) SetupElement(goldObj, "GoldIcon", new Vector2(-150, 0), fontAsset, "Assets/Tiny RPG Forest/Artwork/sprites/misc/coin/coin-1.png");
            
            var diaObj = eHud.Find("Diamond");
            if (diaObj != null) SetupElement(diaObj, "DiamondIcon", new Vector2(0, 0), fontAsset, "Assets/Tiny RPG Forest/Artwork/sprites/misc/gem/gem-1.png");
        }

        if (tHud != null)
        {
            var rt = tHud.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1); rt.anchorMax = new Vector2(0.5f, 1); rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -40); rt.sizeDelta = new Vector2(400, 60);

            var icon = tHud.parent.Find("DayNightIcon");
            if (icon == null) icon = tHud.Find("DayNightIcon");
            if (icon != null)
            {
                icon.SetParent(tHud, false);
                var iRt = icon.GetComponent<RectTransform>();
                iRt.anchorMin = iRt.anchorMax = iRt.pivot = new Vector2(0.5f, 0.5f);
                iRt.anchoredPosition = new Vector2(-120, 0); iRt.sizeDelta = new Vector2(40, 40);
                
                var img = icon.GetComponent<Image>();
                // Temporarily using coin image just to ensure visibility, since UI-15.png wasn't loading right
                img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Tiny RPG Forest/Artwork/sprites/misc/coin/coin-1.png"); 
                img.color = Color.white;
            }

            var timeTxt = tHud.Find("TimeText");
            if (timeTxt != null) SetupTextOnly(timeTxt, new Vector2(-20, 0), fontAsset);

            var dayTxt = tHud.Find("DayText");
            if (dayTxt != null) SetupTextOnly(dayTxt, new Vector2(120, 0), fontAsset);
        }
        
        Debug.Log("ApplyFarmUI_Phase5 completed.");
    }

    private static void SetupElement(Transform obj, string iconName, Vector2 pos, TMP_FontAsset font, string spritePath)
    {
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(100, 40);
        rt.localScale = Vector3.one;
        rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0);

        var t = obj.GetComponent<TextMeshProUGUI>();
        if (t)
        {
            t.font = font; t.alignment = TextAlignmentOptions.Left; t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Overflow; t.fontSize = 32; t.color = Color.white;
        }

        var icon = obj.Find(iconName);
        if (icon != null)
        {
            var iRt = icon.GetComponent<RectTransform>();
            iRt.anchorMin = iRt.anchorMax = iRt.pivot = new Vector2(0, 0.5f);
            iRt.anchoredPosition = new Vector2(-40, 0); iRt.sizeDelta = new Vector2(35, 35);
            iRt.localScale = Vector3.one;
            iRt.localPosition = new Vector3(iRt.localPosition.x, iRt.localPosition.y, 0);
            
            var img = icon.GetComponent<Image>();
            img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            img.color = Color.white;
        }
    }

    private static void SetupTextOnly(Transform obj, Vector2 pos, TMP_FontAsset font)
    {
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(150, 40);
        rt.localScale = Vector3.one;
        rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0);

        var t = obj.GetComponent<TextMeshProUGUI>();
        if (t)
        {
            t.font = font; t.alignment = TextAlignmentOptions.Center; t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Overflow; t.fontSize = 32; t.color = Color.white;
        }
    }
}
