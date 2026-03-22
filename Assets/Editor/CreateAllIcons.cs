using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class CreateAllIcons
{
    [MenuItem("Tools/Moonlit Garden/Create All Icons (16x16)")]
    public static void CreateIcons()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found!");
            return;
        }

        Sprite sunSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/SunIcon.png");
        Sprite lightningSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/LightningIcon.png");
        Sprite moonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/MoonIcon.png");

        if (sunSprite == null) Debug.LogError("SunIcon.png not found!");
        if (lightningSprite == null) Debug.LogError("LightningIcon.png not found!");
        if (moonSprite == null) Debug.LogWarning("MoonIcon.png not found - will use SunIcon");

        // Find or create StaminaIcon
        Transform staminaHud = canvas.transform.Find("StaminaHUD");
        if (staminaHud != null)
        {
            Transform existing = staminaHud.Find("StaminaIcon");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            GameObject iconObj = new GameObject("StaminaIcon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(staminaHud, false);
            
            RectTransform rect = iconObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(5, 0);
            rect.sizeDelta = new Vector2(16, 16); // Small size
            
            Image img = iconObj.GetComponent<Image>();
            img.sprite = lightningSprite;
            img.preserveAspect = true;
            
            // Adjust slider
            Transform slider = staminaHud.Find("StaminaSlider");
            if (slider != null)
            {
                RectTransform sRect = slider.GetComponent<RectTransform>();
                sRect.anchorMin = new Vector2(0, 0);
                sRect.anchorMax = new Vector2(1, 1);
                sRect.offsetMin = new Vector2(24, 5);
                sRect.offsetMax = new Vector2(-10, -5);
            }
            
            Debug.Log("StaminaIcon created (16x16)");
        }

        // Find or create WeatherIcon
        Transform timeHud = canvas.transform.Find("TimeHUD");
        if (timeHud != null)
        {
            Transform existing = timeHud.Find("WeatherIcon");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            GameObject iconObj = new GameObject("WeatherIcon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(timeHud, false);
            
            RectTransform rect = iconObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(5, 5); // Slightly higher
            rect.sizeDelta = new Vector2(16, 16); // Small size
            
            Image img = iconObj.GetComponent<Image>();
            img.sprite = sunSprite; // Default to sun
            img.preserveAspect = true;
            
            Debug.Log("WeatherIcon created (16x16)");
        }

        Debug.Log("All icons created successfully!");
    }
}
