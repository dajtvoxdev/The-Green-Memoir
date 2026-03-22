using UnityEngine;

/// <summary>
/// Persists player settings (BGM volume, SFX volume, fullscreen) to PlayerPrefs
/// and applies them to AudioManager + Screen on startup.
///
/// Phase 3 Feature (#34): Settings Menu.
///
/// Usage:
///   SettingsManager.Instance.SetBGMVolume(0.5f);
///   SettingsManager.Instance.SetFullscreen(true);
/// </summary>
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private const string KEY_BGM        = "setting_bgm_volume";
    private const string KEY_SFX        = "setting_sfx_volume";
    private const string KEY_FULLSCREEN = "setting_fullscreen";

    public float BGMVolume   { get; private set; } = 0.5f;
    public float SFXVolume   { get; private set; } = 0.7f;
    public bool  IsFullscreen { get; private set; } = false;

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
        Load();
        Apply();
    }

    // ==================== PUBLIC API ====================

    /// <summary>Sets BGM volume (0–1) and saves immediately.</summary>
    public void SetBGMVolume(float value)
    {
        BGMVolume = Mathf.Clamp01(value);
        AudioManager.Instance?.SetBGMVolume(BGMVolume);
        PlayerPrefs.SetFloat(KEY_BGM, BGMVolume);
    }

    /// <summary>Sets SFX volume (0–1) and saves immediately.</summary>
    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp01(value);
        AudioManager.Instance?.SetSFXVolume(SFXVolume);
        PlayerPrefs.SetFloat(KEY_SFX, SFXVolume);
    }

    /// <summary>Toggles fullscreen and saves immediately.</summary>
    public void SetFullscreen(bool fullscreen)
    {
        IsFullscreen = fullscreen;
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt(KEY_FULLSCREEN, fullscreen ? 1 : 0);
    }

    // ==================== PRIVATE ====================

    private void Load()
    {
        BGMVolume    = PlayerPrefs.GetFloat(KEY_BGM, 0.5f);
        SFXVolume    = PlayerPrefs.GetFloat(KEY_SFX, 0.7f);
        IsFullscreen = PlayerPrefs.GetInt(KEY_FULLSCREEN, 0) == 1;
    }

    /// <summary>
    /// Applies all loaded settings. Called once after Load().
    /// AudioManager may not exist yet in Awake — deferred to Start().
    /// </summary>
    private void Apply()
    {
        AudioManager.Instance?.SetBGMVolume(BGMVolume);
        AudioManager.Instance?.SetSFXVolume(SFXVolume);
        Screen.fullScreen = IsFullscreen;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
