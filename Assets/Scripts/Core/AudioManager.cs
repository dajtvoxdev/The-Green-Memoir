using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Centralized audio manager for BGM and SFX.
/// Supports time-of-day BGM switching, volume control, and fade transitions.
///
/// Phase 2 Feature (#30): Audio System.
///
/// Usage:
///   AudioManager.Instance.PlaySFX("harvest");
///   AudioManager.Instance.PlayBGM("day_theme");
///   AudioManager.Instance.SetBGMVolume(0.5f);
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("AudioSource for background music (looping).")]
    public AudioSource bgmSource;

    [Tooltip("AudioSource for sound effects (one-shot).")]
    public AudioSource sfxSource;

    [Header("Audio Mixer (Optional)")]
    [Tooltip("AudioMixer for volume control groups.")]
    public AudioMixer audioMixer;

    [Header("BGM Clips")]
    [Tooltip("Background music for daytime.")]
    public AudioClip bgmDay;

    [Tooltip("Background music for nighttime.")]
    public AudioClip bgmNight;

    [Header("SFX Clips")]
    public AudioClip sfxTill;
    public AudioClip sfxPlant;
    public AudioClip sfxWater;
    public AudioClip sfxHarvest;
    public AudioClip sfxBuy;
    public AudioClip sfxSell;
    public AudioClip sfxEquip;
    public AudioClip sfxNotification;
    public AudioClip sfxUIClick;
    public AudioClip sfxNewDay;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float bgmVolume = 0.5f;

    [Range(0f, 1f)]
    public float sfxVolume = 0.7f;

    [Tooltip("BGM crossfade duration in seconds.")]
    public float bgmFadeDuration = 2f;

    /// <summary>
    /// SFX lookup by name for easy access.
    /// </summary>
    private Dictionary<string, AudioClip> sfxRegistry;

    private Coroutine bgmFadeCoroutine;
    private bool isNightBGM;

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
        BuildSFXRegistry();
        SetupAudioSources();
        SubscribeToTimeEvents();

        // Start with day BGM
        if (bgmDay != null)
        {
            PlayBGMImmediate(bgmDay);
        }
    }

    /// <summary>
    /// Builds the name → AudioClip dictionary for SFX.
    /// </summary>
    private void BuildSFXRegistry()
    {
        sfxRegistry = new Dictionary<string, AudioClip>();

        RegisterSFX("till", sfxTill);
        RegisterSFX("plant", sfxPlant);
        RegisterSFX("water", sfxWater);
        RegisterSFX("harvest", sfxHarvest);
        RegisterSFX("buy", sfxBuy);
        RegisterSFX("sell", sfxSell);
        RegisterSFX("equip", sfxEquip);
        RegisterSFX("notification", sfxNotification);
        RegisterSFX("ui_click", sfxUIClick);
        RegisterSFX("new_day", sfxNewDay);
    }

    private void RegisterSFX(string name, AudioClip clip)
    {
        if (clip != null)
        {
            sfxRegistry[name] = clip;
        }
    }

    /// <summary>
    /// Ensures AudioSources exist and are configured correctly.
    /// </summary>
    private void SetupAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        bgmSource.volume = bgmVolume;
        bgmSource.loop = true;

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        sfxSource.volume = sfxVolume;

        // Assign mixer groups if available
        if (audioMixer != null)
        {
            var bgmGroup = audioMixer.FindMatchingGroups("BGM");
            if (bgmGroup.Length > 0) bgmSource.outputAudioMixerGroup = bgmGroup[0];

            var sfxGroup = audioMixer.FindMatchingGroups("SFX");
            if (sfxGroup.Length > 0) sfxSource.outputAudioMixerGroup = sfxGroup[0];
        }
    }

    /// <summary>
    /// Subscribes to GameTimeManager events for time-of-day BGM switching.
    /// </summary>
    private void SubscribeToTimeEvents()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnHourChanged += OnHourChanged;
        }
    }

    private void OnHourChanged(int hour)
    {
        bool shouldBeNight = hour >= 20 || hour < 6;

        if (shouldBeNight && !isNightBGM && bgmNight != null)
        {
            CrossfadeToBGM(bgmNight);
            isNightBGM = true;
        }
        else if (!shouldBeNight && isNightBGM && bgmDay != null)
        {
            CrossfadeToBGM(bgmDay);
            isNightBGM = false;
        }
    }

    // ==================== PUBLIC API ====================

    /// <summary>
    /// Plays a sound effect by name.
    /// </summary>
    public void PlaySFX(string sfxName)
    {
        if (sfxRegistry.TryGetValue(sfxName, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    /// <summary>
    /// Plays a sound effect from a clip directly.
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    /// <summary>
    /// Plays BGM with a crossfade from the current track.
    /// </summary>
    public void PlayBGM(string bgmName)
    {
        AudioClip clip = bgmName switch
        {
            "day" or "day_theme" => bgmDay,
            "night" or "night_theme" => bgmNight,
            _ => null
        };

        if (clip != null)
        {
            CrossfadeToBGM(clip);
        }
    }

    /// <summary>
    /// Plays BGM immediately without fade.
    /// </summary>
    public void PlayBGMImmediate(AudioClip clip)
    {
        if (clip == null) return;

        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    /// <summary>
    /// Crossfades to a new BGM clip.
    /// </summary>
    public void CrossfadeToBGM(AudioClip newClip)
    {
        if (newClip == null || newClip == bgmSource.clip) return;

        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
        }

        bgmFadeCoroutine = StartCoroutine(CrossfadeCoroutine(newClip));
    }

    private IEnumerator CrossfadeCoroutine(AudioClip newClip)
    {
        float halfDuration = bgmFadeDuration / 2f;

        // Fade out
        float startVolume = bgmSource.volume;
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / halfDuration);
            yield return null;
        }

        // Switch clip
        bgmSource.clip = newClip;
        bgmSource.Play();

        // Fade in
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(0f, bgmVolume, elapsed / halfDuration);
            yield return null;
        }

        bgmSource.volume = bgmVolume;
        bgmFadeCoroutine = null;
    }

    /// <summary>
    /// Stops the BGM with a fade out.
    /// </summary>
    public void StopBGM()
    {
        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
        }

        bgmFadeCoroutine = StartCoroutine(FadeOutBGM());
    }

    private IEnumerator FadeOutBGM()
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;
        while (elapsed < bgmFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / bgmFadeDuration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = bgmVolume;
        bgmFadeCoroutine = null;
    }

    // ==================== VOLUME CONTROL ====================

    /// <summary>
    /// Sets BGM volume (0-1).
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;

        if (audioMixer != null)
        {
            float db = volume > 0.001f ? Mathf.Log10(volume) * 20f : -80f;
            audioMixer.SetFloat("BGMVolume", db);
        }
    }

    /// <summary>
    /// Sets SFX volume (0-1).
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;

        if (audioMixer != null)
        {
            float db = volume > 0.001f ? Mathf.Log10(volume) * 20f : -80f;
            audioMixer.SetFloat("SFXVolume", db);
        }
    }

    /// <summary>
    /// Mutes/unmutes all audio.
    /// </summary>
    public void SetMute(bool muted)
    {
        bgmSource.mute = muted;
        sfxSource.mute = muted;
    }

    void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnHourChanged -= OnHourChanged;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
