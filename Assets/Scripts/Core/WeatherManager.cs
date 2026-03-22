using System;
using UnityEngine;

/// <summary>
/// Manages in-game weather: randomizes state each day, exposes IsRainingToday,
/// and fires events for other systems to react.
///
/// Phase 3 Feature (#36): Weather System.
///
/// Weather probabilities per day: Sunny 50%, Cloudy 30%, Rainy 20%.
/// When raining, CropGrowthManager skips the watering requirement automatically.
///
/// Events:
///   OnWeatherChanged(WeatherType) — subscribe to update VFX / HUD
/// </summary>
public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance { get; private set; }

    public enum WeatherType { Sunny, Cloudy, Rainy }

    /// <summary>Current day's weather.</summary>
    public WeatherType TodayWeather { get; private set; } = WeatherType.Sunny;

    /// <summary>True when it is currently raining (crops don't need manual watering).</summary>
    public bool IsRainingToday => TodayWeather == WeatherType.Rainy;

    /// <summary>Fired every time weather changes (new day or initial load).</summary>
    public event Action<WeatherType> OnWeatherChanged;

    [Header("Probabilities (must sum to 1)")]
    [Range(0f, 1f)] public float sunnychance  = 0.5f;
    [Range(0f, 1f)] public float cloudyChance = 0.3f;
    // Rainy chance = 1 - sunny - cloudy

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        RollWeather();

        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnNewDay += HandleNewDay;
    }

    // ==================== PUBLIC ====================

    public string GetWeatherIcon()
    {
        return TodayWeather switch
        {
            WeatherType.Sunny  => "Nang",
            WeatherType.Cloudy => "Co may",
            WeatherType.Rainy  => "Mua",
            _                  => ""
        };
    }

    // ==================== PRIVATE ====================

    private void HandleNewDay(int day)
    {
        RollWeather();
    }

    private void RollWeather()
    {
        float roll = UnityEngine.Random.value;

        WeatherType prev = TodayWeather;

        if (roll < sunnychance)
            TodayWeather = WeatherType.Sunny;
        else if (roll < sunnychance + cloudyChance)
            TodayWeather = WeatherType.Cloudy;
        else
            TodayWeather = WeatherType.Rainy;

        OnWeatherChanged?.Invoke(TodayWeather);

        if (TodayWeather == WeatherType.Rainy)
        {
            NotificationManager.Instance?.ShowNotification(
                "Hôm nay trời mưa! Cây trồng sẽ được tưới tự động.", 3.5f);
        }
        else if (TodayWeather != prev)
        {
            string icon = GetWeatherIcon();
            NotificationManager.Instance?.ShowNotification($"Thời tiết hôm nay: {icon}", 2.5f);
        }

        Debug.Log($"WeatherManager: Day weather = {TodayWeather}");
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnNewDay -= HandleNewDay;
    }
}
