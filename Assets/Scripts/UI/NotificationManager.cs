using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Shows temporary toast notifications at the top of the screen.
/// Uses CanvasGroup alpha for visibility instead of SetActive to avoid
/// Canvas rebuild errors when triggered from UI event handlers.
///
/// Phase 2 Feature (#13): Popup/notification system.
///
/// Usage:
///   NotificationManager.Instance.ShowNotification("Đã mua 1x Cà Chua!");
///   NotificationManager.Instance.ShowNotification("Ngày 2 bắt đầu!", 3f);
/// </summary>
public class NotificationManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static NotificationManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("The notification panel root (with CanvasGroup).")]
    public GameObject notificationPanel;

    [Tooltip("The text component to display messages.")]
    public TMP_Text notificationText;

    [Header("Settings")]
    [Tooltip("Default duration in seconds before notification fades.")]
    public float defaultDuration = 2.5f;

    [Tooltip("Fade speed.")]
    public float fadeSpeed = 3f;

    private CanvasGroup canvasGroup;
    private Coroutine activeNotification;

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

        EnsureCanvasGroup();
    }

    void Start()
    {
        EnsureCanvasGroup();
    }

    /// <summary>
    /// Lazily initializes the CanvasGroup — safe to call multiple times.
    /// Keeps the panel always active but invisible via alpha=0 + blocksRaycasts=false.
    /// </summary>
    private void EnsureCanvasGroup()
    {
        if (canvasGroup != null || notificationPanel == null) return;

        canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
        }

        // Keep panel always active — control visibility via CanvasGroup only.
        // This avoids "graphic rebuild while inside a graphic rebuild loop" errors.
        notificationPanel.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    /// <summary>
    /// Shows a notification message for the specified duration.
    /// </summary>
    public void ShowNotification(string message, float duration = 0f)
    {
        if (notificationPanel == null || notificationText == null)
        {
            Debug.Log($"[Notification] {message}");
            return;
        }

        EnsureCanvasGroup();

        if (duration <= 0f) duration = defaultDuration;

        // Cancel any existing notification
        if (activeNotification != null)
        {
            StopCoroutine(activeNotification);
        }

        activeNotification = StartCoroutine(ShowNotificationCoroutine(message, duration));
    }

    private IEnumerator ShowNotificationCoroutine(string message, float duration)
    {
        EnsureCanvasGroup();
        if (canvasGroup == null) yield break;

        // Set text (panel is already active, just invisible)
        notificationText.text = message;

        // Fade in
        canvasGroup.blocksRaycasts = false;
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Wait
        yield return new WaitForSecondsRealtime(duration);

        // Fade out
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }
        canvasGroup.alpha = 0f;

        activeNotification = null;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
