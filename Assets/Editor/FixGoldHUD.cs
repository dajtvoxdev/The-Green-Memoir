using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class FixGoldHUD : EditorWindow
{
    [MenuItem("Tools/Moonlit Garden/Fix Gold HUD")]
    public static void FixGoldUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas!");
            return;
        }

        // Tìm GoldHUD
        Transform goldHud = canvas.transform.Find("GoldHUD");
        if (goldHud == null)
        {
            Debug.LogError("Không tìm thấy GoldHUD!");
            return;
        }

        // Xóa icon cũ nếu có
        Transform oldIcon = goldHud.Find("Icon");
        if (oldIcon != null)
            DestroyImmediate(oldIcon.gameObject);

        // Load sprite
        Sprite goldSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/GoldIcon.png");

        // Tạo icon mới
        GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(goldHud, false);
        
        RectTransform iconRt = icon.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0, 0.5f);
        iconRt.anchorMax = new Vector2(0, 0.5f);
        iconRt.pivot = new Vector2(0, 0.5f);
        iconRt.anchoredPosition = new Vector2(5, 0);
        iconRt.sizeDelta = new Vector2(30, 30);

        Image iconImg = icon.GetComponent<Image>();
        if (goldSprite != null)
        {
            iconImg.sprite = goldSprite;
            iconImg.color = Color.white;
        }
        else
        {
            iconImg.color = new Color32(255, 215, 0, 255);
        }

        Debug.Log("Đã fix GoldHUD với icon!");
    }
}
