using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class AddWeatherStaminaIcons
{
    [MenuItem("Tools/Moonlit Garden/Add Weather & Stamina Icons")]
    public static void AddIcons()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found!");
            return;
        }

        // Load sprites
        Sprite sunSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/SunIcon.png");
        Sprite lightningSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/LightningIcon.png");

        if (sunSprite == null)
        {
            Debug.LogError("SunIcon.png not found at Assets/UI/");
            return;
        }
        if (lightningSprite == null)
        {
            Debug.LogError("LightningIcon.png not found at Assets/UI/");
            return;
        }

        // Find TimeHUD and add Sun icon
        Transform timeHud = canvas.transform.Find("TimeHUD");
        if (timeHud != null)
        {
            // Check if icon already exists
            Transform existingIcon = timeHud.Find("WeatherIcon");
            if (existingIcon != null)
            {
                Object.DestroyImmediate(existingIcon.gameObject);
            }

            GameObject iconObj = new GameObject("WeatherIcon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(timeHud, false);
            
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.pivot = new Vector2(0, 0.5f);
            iconRect.anchoredPosition = new Vector2(5, 0);
            iconRect.sizeDelta = new Vector2(24, 24);
            
            Image iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = sunSprite;
            iconImg.preserveAspect = true;
            
            // Adjust TimeText position
            Transform timeText = timeHud.Find("TimeText");
            if (timeText != null)
            {
                RectTransform textRect = timeText.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0, 0.5f);
                textRect.anchorMax = new Vector2(1, 0.5f);
                textRect.pivot = new Vector2(0.5f, 0.5f);
                textRect.offsetMin = new Vector2(32, -15);
                textRect.offsetMax = new Vector2(-5, 15);
            }
            
            Debug.Log("Sun icon added to TimeHUD");
        }

        // Find StaminaHUD and add Lightning icon
        Transform staminaHud = canvas.transform.Find("StaminaHUD");
        if (staminaHud != null)
        {
            // Check if icon already exists
            Transform existingIcon = staminaHud.Find("StaminaIcon");
            if (existingIcon != null)
            {
                Object.DestroyImmediate(existingIcon.gameObject);
            }

            GameObject iconObj = new GameObject("StaminaIcon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(staminaHud, false);
            
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.pivot = new Vector2(0, 0.5f);
            iconRect.anchoredPosition = new Vector2(5, 0);
            iconRect.sizeDelta = new Vector2(24, 24);
            
            Image iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = lightningSprite;
            iconImg.preserveAspect = true;
            
            // Remove Label text if exists (we have icon now)
            Transform label = staminaHud.Find("Label");
            if (label != null)
            {
                Object.DestroyImmediate(label.gameObject);
            }
            
            // Adjust slider position
            Transform slider = staminaHud.Find("StaminaSlider");
            if (slider != null)
            {
                RectTransform sliderRect = slider.GetComponent<RectTransform>();
                sliderRect.anchorMin = new Vector2(0, 0);
                sliderRect.anchorMax = new Vector2(1, 1);
                sliderRect.offsetMin = new Vector2(32, 5);
                sliderRect.offsetMax = new Vector2(-10, -5);
            }
            
            Debug.Log("Lightning icon added to StaminaHUD");
        }

        Debug.Log("Icons added successfully!");
    }
}
