using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the Settings panel UI: BGM/SFX sliders, fullscreen toggle, close button.
/// Extends PanelBase for integration with UIManager.
///
/// Phase 3 Feature (#34): Settings Menu.
///
/// Key binding: ESC to open/close.
/// Wire: bgmSlider, sfxSlider, fullscreenToggle, bgmValueText, sfxValueText in Inspector.
/// </summary>
public class SettingsPanel : PanelBase
{
    [Header("UI References")]
    [Tooltip("Root panel GameObject reference (usually this gameObject).")]
    public GameObject panelRoot;

    [Tooltip("Slider for BGM volume (0-1).")]
    public Slider bgmSlider;

    [Tooltip("Slider for SFX volume (0-1).")]
    public Slider sfxSlider;

    [Tooltip("Toggle for fullscreen mode.")]
    public Toggle fullscreenToggle;

    [Tooltip("Text showing BGM volume as percentage.")]
    public TMP_Text bgmValueText;

    [Tooltip("Text showing SFX volume as percentage.")]
    public TMP_Text sfxValueText;

    [Header("Keys")]
    [Tooltip("Key to open/close settings.")]
    public KeyCode toggleKey = KeyCode.Escape;

    private bool isOpen = false;

    protected override void Awake()
    {
        base.Awake();
        
        panelId = "settings";
        pauseGameWhenOpen = true;
        closeOnEscape = true;
    }

    void Start()
    {
        SyncToSettings();
        
        // Ensure panel starts hidden
        if (!isOpen)
        {
            SetVisibleImmediate(false);
            if (panelRoot != null && panelRoot != gameObject)
                panelRoot.SetActive(false);
        }
    }

    void Update()
    {
        base.Update();
        
        // Handle toggle key
        if (Input.GetKeyDown(toggleKey) && !AnyOtherPanelBlocking())
        {
            TogglePanel();
        }
    }

    // ==================== PUBLIC ====================

    public void TogglePanel()
    {
        if (IsVisible)
            ClosePanel();
        else
            ShowPanel();
    }

    public void ShowPanel()
    {
        if (panelRoot != null && panelRoot != gameObject)
            panelRoot.SetActive(true);
        
        Show();
        isOpen = true;
    }

    public void ClosePanel()
    {
        Hide();
        isOpen = false;
    }

    public override void Show()
    {
        base.Show();
        SyncToSettings();
        isOpen = true;
    }

    public override void Hide()
    {
        base.Hide();
        isOpen = false;
    }

    // ==================== PRIVATE ====================

    /// <summary>
    /// Syncs slider/toggle values from SettingsManager and registers callbacks.
    /// </summary>
    private void SyncToSettings()
    {
        if (SettingsManager.Instance == null) return;

        // Remove existing listeners to avoid duplicates
        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveListener(OnBGMChanged);
            bgmSlider.value = SettingsManager.Instance.BGMVolume;
            bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
            sfxSlider.value = SettingsManager.Instance.SFXVolume;
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
            fullscreenToggle.isOn = SettingsManager.Instance.IsFullscreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        UpdateValueTexts();
    }

    private void OnBGMChanged(float value)
    {
        SettingsManager.Instance?.SetBGMVolume(value);
        UpdateValueTexts();
    }

    private void OnSFXChanged(float value)
    {
        SettingsManager.Instance?.SetSFXVolume(value);
        UpdateValueTexts();
    }

    private void OnFullscreenChanged(bool value)
    {
        SettingsManager.Instance?.SetFullscreen(value);
    }

    private void UpdateValueTexts()
    {
        if (bgmValueText != null && bgmSlider != null)
            bgmValueText.text = Mathf.RoundToInt(bgmSlider.value * 100) + "%";

        if (sfxValueText != null && sfxSlider != null)
            sfxValueText.text = Mathf.RoundToInt(sfxSlider.value * 100) + "%";
    }

    private bool AnyOtherPanelBlocking()
    {
        // Check if other panels are open that might block settings
        if (UIManager.Instance != null)
        {
            // Allow settings to open over other panels, but check for specific blocking states
            // like dialogue which pauses time
        }
        return false;
    }

    void OnDestroy()
    {
        // Clean up listeners
        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(OnBGMChanged);
        
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
        
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
    }
}
