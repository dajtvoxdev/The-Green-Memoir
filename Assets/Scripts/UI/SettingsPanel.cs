using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase.Auth;

/// <summary>
/// Controls the Settings panel UI: BGM/SFX sliders, audio toggles, fullscreen toggle,
/// logout button. Works with PanelBase's CanvasGroup fade system.
/// ESC key toggles the panel. GO stays active so Update() can detect ESC.
/// </summary>
public class SettingsPanel : PanelBase
{
    [Header("UI References")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Toggle fullscreenToggle;
    public TMP_Text bgmValueText;
    public TMP_Text sfxValueText;

    [Header("Audio Toggles")]
    public Toggle bgmToggle;
    public Toggle sfxToggle;

    [Header("Buttons")]
    public Button logoutButton;
    public Button closeButton;

    [Header("Keys")]
    public KeyCode toggleKey = KeyCode.Escape;

    private bool isOpen = false;
    private bool isLoggingOut = false;

    protected override void Awake()
    {
        panelId = "settings";
        pauseGameWhenOpen = true;
        closeOnEscape = false; // We handle ESC ourselves

        // Call base.Awake() so PanelBase initializes its CanvasGroup and targetAlpha
        base.Awake();

        // PanelBase.Awake() calls SetVisibleImmediate(false) which deactivates the GO.
        // We need the GO active for ESC detection in Update, so reactivate it
        // but keep CanvasGroup hidden (alpha=0, non-interactable).
        gameObject.SetActive(true);
    }

    void Start()
    {
        isOpen = false;
        WireCloseButton();
        SyncToSettings();
    }

    protected override void Update()
    {
        // Let PanelBase handle fade animation
        base.Update();

        // Handle ESC toggle
        if (Input.GetKeyDown(toggleKey) && !AnyOtherPanelBlocking())
        {
            TogglePanel();
        }
    }

    // ==================== PUBLIC ====================

    public void TogglePanel()
    {
        if (isOpen)
            ClosePanel();
        else
            ShowPanel();
    }

    public void ShowPanel()
    {
        Debug.Log("SettingsPanel: ShowPanel called");

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        // Use PanelBase's Show() which properly sets targetAlpha=1 and handles fade
        base.Show();
        SyncToSettings();
        isOpen = true;

        AudioManager.Instance?.PlaySFX("ui_click");
    }

    public void ClosePanel()
    {
        Debug.Log("SettingsPanel: ClosePanel called");

        // Use PanelBase's Hide() which sets targetAlpha=0 and handles fade
        base.Hide();
        isOpen = false;

        // Keep GO active for ESC detection (PanelBase.Hide may deactivate it)
        gameObject.SetActive(true);
    }

    public override void Show()
    {
        ShowPanel();
    }

    public override void Hide()
    {
        ClosePanel();
    }

    // ==================== PRIVATE ====================

    private void WireCloseButton()
    {
        if (closeButton == null)
        {
            var closeT = transform.Find("CloseButton");
            if (closeT != null) closeButton = closeT.GetComponent<Button>();
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    private void SyncToSettings()
    {
        if (SettingsManager.Instance == null) return;

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

        if (bgmToggle != null)
        {
            bgmToggle.onValueChanged.RemoveListener(OnBGMToggleChanged);
            bgmToggle.isOn = !SettingsManager.Instance.IsBGMMuted;
            bgmToggle.onValueChanged.AddListener(OnBGMToggleChanged);
        }

        if (sfxToggle != null)
        {
            sfxToggle.onValueChanged.RemoveListener(OnSFXToggleChanged);
            sfxToggle.isOn = !SettingsManager.Instance.IsSFXMuted;
            sfxToggle.onValueChanged.AddListener(OnSFXToggleChanged);
        }

        if (logoutButton != null)
        {
            logoutButton.onClick.RemoveListener(OnLogoutClicked);
            logoutButton.onClick.AddListener(OnLogoutClicked);
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

    private void OnBGMToggleChanged(bool isOn)
    {
        SettingsManager.Instance?.SetBGMMuted(!isOn);
    }

    private void OnSFXToggleChanged(bool isOn)
    {
        SettingsManager.Instance?.SetSFXMuted(!isOn);
    }

    private void OnLogoutClicked()
    {
        if (isLoggingOut) return;
        StartCoroutine(LogoutRoutine());
    }

    private System.Collections.IEnumerator LogoutRoutine()
    {
        isLoggingOut = true;
        Debug.Log("SettingsPanel: Logout requested. Flushing gameplay data before leaving PlayScene.");

        FindFirstObjectByType<RecyclableInventoryManager>()?.FlushSave();
        PlayerEconomyManager.Instance?.FlushSave();

        bool saveCompleted = false;
        bool saveSucceeded = false;
        string saveError = null;

        if (LoadDataManager.Instance != null && LoadDataManager.userInGame != null && LoadDataManager.firebaseUser != null)
        {
            LoadDataManager.Instance.SaveUserInGame((success, error) =>
            {
                saveCompleted = true;
                saveSucceeded = success;
                saveError = error;
            });

            float timeout = 3f;
            while (!saveCompleted && timeout > 0f)
            {
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }

            if (!saveCompleted)
            {
                Debug.LogWarning("SettingsPanel: Timed out waiting for final profile save during logout.");
            }
            else if (!saveSucceeded)
            {
                Debug.LogWarning($"SettingsPanel: Final profile save reported an error during logout: {saveError}");
            }
        }

        yield return new WaitForSecondsRealtime(0.2f);

        FirebaseAuth.DefaultInstance.SignOut();
        LoadDataManager.Reset();
        Time.timeScale = 1f;
        SceneManager.LoadScene("LoginScene");
        Debug.Log("SettingsPanel: Logged out, returning to LoginScene.");
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
        return false;
    }

    void OnDestroy()
    {
        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(OnBGMChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        if (bgmToggle != null)
            bgmToggle.onValueChanged.RemoveListener(OnBGMToggleChanged);
        if (sfxToggle != null)
            sfxToggle.onValueChanged.RemoveListener(OnSFXToggleChanged);
        if (logoutButton != null)
            logoutButton.onClick.RemoveListener(OnLogoutClicked);
        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePanel);
    }
}
