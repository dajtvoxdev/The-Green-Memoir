using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Controls the global Light2D to create a day-night cycle.
/// Reads time from GameTimeManager and interpolates light color/intensity.
/// Phase 2 Feature (#5): Day-Night cycle with URP 2D.
///
/// Lighting phases:
///   Night  (0:00-4:59)  — dark blue, low intensity
///   Dawn   (5:00-7:59)  — warm yellow/orange transition
///   Day    (8:00-16:59) — bright white
///   Dusk   (17:00-19:59)— orange/red transition
///   Night  (20:00-23:59)— dark blue, low intensity
/// </summary>
public class DayNightController : MonoBehaviour
{
    [Header("Light Reference")]
    [Tooltip("The global Light2D that illuminates the scene.")]
    public Light2D globalLight;

    [Header("Lighting Presets")]
    [Tooltip("Color at midnight (darkest point).")]
    public Color nightColor = new Color(0.15f, 0.15f, 0.35f, 1f);

    [Tooltip("Color at dawn (warm sunrise).")]
    public Color dawnColor = new Color(0.95f, 0.75f, 0.4f, 1f);

    [Tooltip("Color at midday (full daylight).")]
    public Color dayColor = new Color(1f, 1f, 0.95f, 1f);

    [Tooltip("Color at dusk (warm sunset).")]
    public Color duskColor = new Color(0.9f, 0.5f, 0.3f, 1f);

    [Header("Intensity")]
    [Tooltip("Light intensity during night.")]
    public float nightIntensity = 0.3f;

    [Tooltip("Light intensity at dawn.")]
    public float dawnIntensity = 0.7f;

    [Tooltip("Light intensity during day.")]
    public float dayIntensity = 1f;

    [Tooltip("Light intensity at dusk.")]
    public float duskIntensity = 0.6f;

    [Header("Smoothing")]
    [Tooltip("How smoothly the light transitions between phases.")]
    public float transitionSpeed = 2f;

    // Target values for smooth interpolation
    private Color targetColor;
    private float targetIntensity;

    void Start()
    {
        if (globalLight == null)
        {
            globalLight = GetComponent<Light2D>();
        }

        if (globalLight == null)
        {
            Debug.LogError("DayNightController: No Light2D assigned or found!");
            enabled = false;
            return;
        }

        // Set initial lighting based on current time
        UpdateTargetFromTime();
        globalLight.color = targetColor;
        globalLight.intensity = targetIntensity;
    }

    void Update()
    {
        if (GameTimeManager.Instance == null) return;

        UpdateTargetFromTime();

        // Smooth interpolation toward target
        globalLight.color = Color.Lerp(globalLight.color, targetColor, Time.deltaTime * transitionSpeed);
        globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, Time.deltaTime * transitionSpeed);
    }

    /// <summary>
    /// Calculates the target color and intensity based on current game time.
    /// Uses piecewise linear interpolation between the 4 key times of day.
    /// </summary>
    private void UpdateTargetFromTime()
    {
        float hour = GameTimeManager.Instance.TimeOfDayHours;

        // Define key hours for each phase transition
        // Night: 0-5, Dawn: 5-8, Day: 8-17, Dusk: 17-20, Night: 20-24
        if (hour < 5f)
        {
            // Deep night (0:00 - 4:59)
            targetColor = nightColor;
            targetIntensity = nightIntensity;
        }
        else if (hour < 8f)
        {
            // Dawn transition (5:00 - 7:59)
            float t = (hour - 5f) / 3f; // 0 at 5:00, 1 at 8:00
            targetColor = Color.Lerp(nightColor, dawnColor, t * 0.5f);
            if (t > 0.5f)
            {
                targetColor = Color.Lerp(dawnColor, dayColor, (t - 0.5f) * 2f);
            }
            targetIntensity = Mathf.Lerp(nightIntensity, dayIntensity, t);
        }
        else if (hour < 17f)
        {
            // Full daylight (8:00 - 16:59)
            targetColor = dayColor;
            targetIntensity = dayIntensity;
        }
        else if (hour < 20f)
        {
            // Dusk transition (17:00 - 19:59)
            float t = (hour - 17f) / 3f; // 0 at 17:00, 1 at 20:00
            if (t < 0.5f)
            {
                targetColor = Color.Lerp(dayColor, duskColor, t * 2f);
            }
            else
            {
                targetColor = Color.Lerp(duskColor, nightColor, (t - 0.5f) * 2f);
            }
            targetIntensity = Mathf.Lerp(dayIntensity, nightIntensity, t);
        }
        else
        {
            // Night (20:00 - 23:59)
            targetColor = nightColor;
            targetIntensity = nightIntensity;
        }
    }
}
