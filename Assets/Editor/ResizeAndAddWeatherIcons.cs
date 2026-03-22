using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ResizeAndAddWeatherIcons
{
    [MenuItem("Tools/Moonlit Garden/Fix Icon Sizes")]
    public static void FixIconSizes()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Resize StaminaIcon
        Transform staminaIcon = canvas.transform.Find("StaminaHUD/StaminaIcon");
        if (staminaIcon != null)
        {
            RectTransform rect = staminaIcon.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(16, 16);
            rect.anchoredPosition = new Vector2(5, 0);
            Debug.Log("StaminaIcon resized to 16x16");
        }

        // Resize WeatherIcon
        Transform weatherIcon = canvas.transform.Find("TimeHUD/WeatherIcon");
        if (weatherIcon != null)
        {
            RectTransform rect = weatherIcon.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(16, 16);
            rect.anchoredPosition = new Vector2(5, 0);
            Debug.Log("WeatherIcon resized to 16x16");
        }

        Debug.Log("Icon sizes fixed!");
    }
}
