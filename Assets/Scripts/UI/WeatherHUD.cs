using UnityEngine;
using TMPro;

/// <summary>
/// Shows current weather in a HUD text element.
/// Updates whenever WeatherManager fires OnWeatherChanged.
///
/// Phase 3 Feature (#36): Weather System.
///
/// Usage: Attach to a UI GameObject with TMP_Text. Wire weatherText in Inspector.
/// </summary>
public class WeatherHUD : MonoBehaviour
{
    [Tooltip("Text element showing the current weather.")]
    public TMP_Text weatherText;

    void Start()
    {
        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.OnWeatherChanged += UpdateDisplay;
            UpdateDisplay(WeatherManager.Instance.TodayWeather);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void UpdateDisplay(WeatherManager.WeatherType weather)
    {
        if (weatherText == null) return;

        weatherText.text = weather switch
        {
            WeatherManager.WeatherType.Sunny  => "Nang",
            WeatherManager.WeatherType.Cloudy => "Co may",
            WeatherManager.WeatherType.Rainy  => "Mua",
            _                                 => ""
        };

        weatherText.color = weather switch
        {
            WeatherManager.WeatherType.Sunny  => new Color(1f, 0.85f, 0.2f, 1f),
            WeatherManager.WeatherType.Cloudy => new Color(0.7f, 0.7f, 0.75f, 1f),
            WeatherManager.WeatherType.Rainy  => new Color(0.4f, 0.7f, 1f, 1f),
            _                                 => Color.white
        };
    }

    void OnDestroy()
    {
        if (WeatherManager.Instance != null)
            WeatherManager.Instance.OnWeatherChanged -= UpdateDisplay;
    }
}
