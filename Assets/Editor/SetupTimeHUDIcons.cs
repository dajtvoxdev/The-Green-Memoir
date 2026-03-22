using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class SetupTimeHUDIcons
{
    [MenuItem("Tools/Moonlit Garden/Setup TimeHUD Icons")]
    public static void SetupIcons()
    {
        // Find TimeHUD
        TimeHUD timeHud = Object.FindFirstObjectByType<TimeHUD>();
        if (timeHud == null)
        {
            Debug.LogError("TimeHUD not found!");
            return;
        }

        // Load sprites
        Sprite sunSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/SunIcon.png");
        Sprite moonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/MoonIcon.png");

        if (sunSprite == null)
        {
            Debug.LogError("SunIcon.png not found!");
            return;
        }
        if (moonSprite == null)
        {
            Debug.LogError("MoonIcon.png not found!");
            return;
        }

        // Assign sprites to TimeHUD
        timeHud.sunSprite = sunSprite;
        timeHud.moonSprite = moonSprite;

        // Find WeatherIcon in hierarchy
        Transform timeHudTransform = timeHud.transform;
        Transform weatherIconTransform = timeHudTransform.Find("WeatherIcon");
        
        if (weatherIconTransform != null)
        {
            Image weatherIconImage = weatherIconTransform.GetComponent<Image>();
            if (weatherIconImage != null)
            {
                timeHud.weatherIcon = weatherIconImage;
                // Set initial sprite to sun
                weatherIconImage.sprite = sunSprite;
                Debug.Log("WeatherIcon assigned to TimeHUD");
            }
        }
        else
        {
            Debug.LogError("WeatherIcon GameObject not found as child of TimeHUD!");
        }

        EditorUtility.SetDirty(timeHud);
        Debug.Log("TimeHUD icons setup complete!");
    }
}
