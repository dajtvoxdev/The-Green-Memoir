using UnityEngine;
using TMPro;

/// <summary>
/// HUD element that displays the in-game time and day count.
/// Reads from GameTimeManager and updates UI text elements.
///
/// Phase 2 Feature (#5): Visual time display for Day-Night cycle.
/// </summary>
public class TimeHUD : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text showing current time (e.g., '2:30 PM').")]
    public TMP_Text timeText;

    [Tooltip("Text showing current day (e.g., 'Day 3').")]
    public TMP_Text dayText;

    [Tooltip("Text showing current period (e.g., 'Morning').")]
    public TMP_Text periodText;

    [Header("Settings")]
    [Tooltip("Use 24-hour format (14:30) vs 12-hour (2:30 PM).")]
    public bool use24HourFormat = false;

    [Tooltip("Update interval in seconds (lower = smoother but more CPU).")]
    public float updateInterval = 1f;

    [Header("Period Colors")]
    public Color dawnColor = new Color(1f, 0.85f, 0.5f, 1f);
    public Color morningColor = new Color(1f, 0.9f, 0.4f, 1f);
    public Color afternoonColor = new Color(1f, 0.7f, 0.3f, 1f);
    public Color eveningColor = new Color(0.8f, 0.5f, 0.7f, 1f);
    public Color nightColor = new Color(0.4f, 0.5f, 0.8f, 1f);

    private float nextUpdateTime;

    void Update()
    {
        if (GameTimeManager.Instance == null) return;

        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;
            UpdateDisplay();
        }
    }

    /// <summary>
    /// Updates the HUD text elements from GameTimeManager.
    /// </summary>
    private void UpdateDisplay()
    {
        var time = GameTimeManager.Instance;

        if (timeText != null)
        {
            timeText.text = time.GetFormattedTime(use24HourFormat);
        }

        if (dayText != null)
        {
            dayText.text = LocalizationManager.LocalizeText($"Day {time.CurrentDay}");
        }

        if (periodText != null)
        {
            periodText.text = LocalizationManager.LocalizeText(GetPeriodDisplay(time.CurrentPeriod));
            periodText.color = GetPeriodColor(time.CurrentPeriod);
        }
    }

    /// <summary>
    /// Returns a display-friendly name for the time period.
    /// </summary>
    private string GetPeriodDisplay(GameTimeManager.TimePeriod period)
    {
        switch (period)
        {
            case GameTimeManager.TimePeriod.Dawn: return "Dawn";
            case GameTimeManager.TimePeriod.Morning: return "Morning";
            case GameTimeManager.TimePeriod.Afternoon: return "Afternoon";
            case GameTimeManager.TimePeriod.Evening: return "Evening";
            case GameTimeManager.TimePeriod.Night: return "Night";
            default: return "";
        }
    }

    /// <summary>
    /// Returns color for the current time period for visual feedback.
    /// </summary>
    private Color GetPeriodColor(GameTimeManager.TimePeriod period)
    {
        switch (period)
        {
            case GameTimeManager.TimePeriod.Dawn: return dawnColor;
            case GameTimeManager.TimePeriod.Morning: return morningColor;
            case GameTimeManager.TimePeriod.Afternoon: return afternoonColor;
            case GameTimeManager.TimePeriod.Evening: return eveningColor;
            case GameTimeManager.TimePeriod.Night: return nightColor;
            default: return Color.white;
        }
    }
}
