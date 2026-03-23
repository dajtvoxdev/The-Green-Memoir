using System;
using UnityEngine;

/// <summary>
/// Manages in-game time: hours, days, seasons.
/// 1 real second = timeScale in-game minutes (configurable).
/// Phase 2 Feature (#5): Foundation for Day-Night cycle.
/// </summary>
public class GameTimeManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static GameTimeManager Instance { get; private set; }

    [Header("Time Settings")]
    [Tooltip("How many in-game minutes pass per real second.")]
    public float timeScale = 10f;

    [Tooltip("Starting hour (0-23) when the game begins.")]
    public int startHour = 6;

    [Tooltip("Starting day count.")]
    public int startDay = 1;

    [Header("Day Length")]
    [Tooltip("Total in-game hours in one day.")]
    public int hoursPerDay = 24;

    [Header("Current Time (Read Only)")]
    [SerializeField] private int currentDay;
    [SerializeField] private int currentHour;
    [SerializeField] private int currentMinute;
    [SerializeField] private float currentTimeOfDay;

    /// <summary>
    /// Current day number (starts at 1).
    /// </summary>
    public int CurrentDay => currentDay;

    /// <summary>
    /// Current hour (0-23).
    /// </summary>
    public int CurrentHour => currentHour;

    /// <summary>
    /// Current minute (0-59).
    /// </summary>
    public int CurrentMinute => currentMinute;

    /// <summary>
    /// Normalized time of day (0.0 = midnight, 0.25 = 6AM, 0.5 = noon, 0.75 = 6PM).
    /// Used by DayNightController to interpolate lighting.
    /// </summary>
    public float NormalizedTimeOfDay => currentTimeOfDay / hoursPerDay;

    /// <summary>
    /// Time of day as a float in hours (e.g., 6.5 = 6:30 AM).
    /// </summary>
    public float TimeOfDayHours => currentTimeOfDay;

    /// <summary>
    /// True if it's currently night time (between 20:00 and 5:59).
    /// </summary>
    public bool IsNight => currentHour >= 20 || currentHour < 6;

    /// <summary>
    /// True if the game clock is running.
    /// </summary>
    public bool IsRunning { get; private set; } = true;

    // Internal accumulator for sub-minute fractions
    private float minuteAccumulator;

    /// <summary>
    /// Fired when a new hour begins. Parameter: hour (0-23).
    /// </summary>
    public event Action<int> OnHourChanged;

    /// <summary>
    /// Fired when a new day begins. Parameter: day number.
    /// </summary>
    public event Action<int> OnNewDay;

    /// <summary>
    /// Fired every in-game minute. Parameters: hour, minute.
    /// </summary>
    public event Action<int, int> OnMinuteChanged;

    /// <summary>
    /// Time period for gameplay logic (crop schedules, NPC schedules, etc.).
    /// </summary>
    public enum TimePeriod
    {
        Dawn,       // 5:00 - 7:59
        Morning,    // 8:00 - 11:59
        Afternoon,  // 12:00 - 16:59
        Evening,    // 17:00 - 19:59
        Night       // 20:00 - 4:59
    }

    /// <summary>
    /// Current time period.
    /// </summary>
    public TimePeriod CurrentPeriod
    {
        get
        {
            if (currentHour >= 5 && currentHour < 8) return TimePeriod.Dawn;
            if (currentHour >= 8 && currentHour < 12) return TimePeriod.Morning;
            if (currentHour >= 12 && currentHour < 17) return TimePeriod.Afternoon;
            if (currentHour >= 17 && currentHour < 20) return TimePeriod.Evening;
            return TimePeriod.Night;
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        currentDay = startDay;
        currentTimeOfDay = startHour;
        currentHour = startHour;
        currentMinute = 0;
        minuteAccumulator = 0f;
    }

    void Update()
    {
        if (!IsRunning) return;

        AdvanceTime(Time.deltaTime);
    }

    /// <summary>
    /// Advances time by the given real-time delta.
    /// </summary>
    private void AdvanceTime(float realDeltaSeconds)
    {
        // Convert real seconds to in-game minutes
        float inGameMinutes = realDeltaSeconds * timeScale;
        minuteAccumulator += inGameMinutes;

        // Process whole minutes
        while (minuteAccumulator >= 1f)
        {
            minuteAccumulator -= 1f;
            TickMinute();
        }

        // Update continuous time for smooth lighting interpolation
        currentTimeOfDay = currentHour + (currentMinute / 60f) + (minuteAccumulator / 60f);

        // Ensure time wraps within hoursPerDay
        if (currentTimeOfDay >= hoursPerDay)
        {
            currentTimeOfDay -= hoursPerDay;
        }
    }

    /// <summary>
    /// Called once per in-game minute.
    /// </summary>
    private void TickMinute()
    {
        int oldHour = currentHour;

        currentMinute++;
        if (currentMinute >= 60)
        {
            currentMinute = 0;
            currentHour++;

            if (currentHour >= hoursPerDay)
            {
                currentHour = 0;
                currentDay++;
                OnNewDay?.Invoke(currentDay);
                AudioManager.Instance?.PlaySFX("new_day");
                Debug.Log($"GameTime: New day! Day {currentDay}");

                // Notify player
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowNotification($"Ngày {currentDay} bắt đầu!", 3f);
                }
            }

            OnHourChanged?.Invoke(currentHour);
        }

        OnMinuteChanged?.Invoke(currentHour, currentMinute);
    }

    // ==================== PUBLIC API ====================

    /// <summary>
    /// Pauses the game clock.
    /// </summary>
    public void PauseClock()
    {
        IsRunning = false;
    }

    /// <summary>
    /// Resumes the game clock.
    /// </summary>
    public void ResumeClock()
    {
        IsRunning = true;
    }

    /// <summary>
    /// Sets the time to a specific hour and minute.
    /// </summary>
    public void SetTime(int hour, int minute = 0)
    {
        int oldHour = currentHour;
        currentHour = Mathf.Clamp(hour, 0, hoursPerDay - 1);
        currentMinute = Mathf.Clamp(minute, 0, 59);
        currentTimeOfDay = currentHour + (currentMinute / 60f);
        minuteAccumulator = 0f;

        if (currentHour != oldHour)
        {
            OnHourChanged?.Invoke(currentHour);
        }
    }

    /// <summary>
    /// Skips to the next morning (6:00).
    /// Used for "sleep" mechanic.
    /// </summary>
    public void SkipToMorning()
    {
        currentDay++;
        SetTime(6, 0);
        OnNewDay?.Invoke(currentDay);
        Debug.Log($"GameTime: Skipped to morning of Day {currentDay}");
    }

    /// <summary>
    /// Returns formatted time string (e.g., "14:30" or "2:30 PM").
    /// </summary>
    public string GetFormattedTime(bool use24Hour = false)
    {
        if (use24Hour)
        {
            return $"{currentHour:D2}:{currentMinute:D2}";
        }

        int displayHour = currentHour % 12;
        if (displayHour == 0) displayHour = 12;
        string ampm = currentHour < 12 ? "AM" : "PM";
        return $"{displayHour}:{currentMinute:D2} {ampm}";
    }

    /// <summary>
    /// Returns the total in-game minutes elapsed since day 1.
    /// </summary>
    public int TotalMinutesElapsed => (currentDay - 1) * hoursPerDay * 60 + currentHour * 60 + currentMinute;

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
