using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class AddDiamondHUD : EditorWindow
{
    [MenuItem("Tools/Moonlit Garden/Add Diamond HUD")]
public static void AddDiamondUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas!");
            return;
        }

        // Load sprite từ PixelLab
        Sprite diamondSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/DiamondIcon.png");

        // Tạo DiamondHUD bên cạnh GoldHUD
        GameObject diamondHud = new GameObject("DiamondHUD", typeof(RectTransform));
        diamondHud.transform.SetParent(canvas.transform, false);

        RectTransform rt = diamondHud.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-10, -60); // Bên dưới GoldHUD
        rt.sizeDelta = new Vector2(150, 40);

        Image bg = diamondHud.AddComponent<Image>();
        bg.color = new Color32(60, 40, 30, 200);

        // Icon kim cương từ PixelLab
        GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(diamondHud.transform, false);
        RectTransform iconRt = icon.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0, 0.5f);
        iconRt.anchorMax = new Vector2(0, 0.5f);
        iconRt.pivot = new Vector2(0, 0.5f);
        iconRt.anchoredPosition = new Vector2(5, 0);
        iconRt.sizeDelta = new Vector2(30, 30);
        
        Image iconImg = icon.GetComponent<Image>();
        if (diamondSprite != null)
            iconImg.sprite = diamondSprite;
        else
            iconImg.color = new Color32(100, 200, 255, 255); // Fallback

        // Text số kim cương
        GameObject textObj = new GameObject("DiamondText", typeof(RectTransform));
        textObj.transform.SetParent(diamondHud.transform, false);
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0, 0);
        textRt.anchorMax = new Vector2(1, 1);
        textRt.offsetMin = new Vector2(40, 0);
        textRt.offsetMax = new Vector2(-5, 0);

        TMP_Text tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "0";
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.color = new Color32(255, 245, 220, 255);

        Debug.Log("Đã thêm DiamondHUD với icon PixelLab!");
    }
}
