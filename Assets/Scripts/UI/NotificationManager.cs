using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Shows temporary toast notifications at the top of the screen.
/// Used for harvest results, errors, time events, etc.
///
/// Phase 2 Feature (#13): Popup/notification system.
///
/// Usage:
///   NotificationManager.Instance.ShowNotification("Harvested 3x Tomato!");
///   NotificationManager.Instance.ShowNotification("Day 2 started!", 3f);
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
    }

    void Start()
    {
        if (notificationPanel != null)
        {
            canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
            notificationPanel.SetActive(false);
        }
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
        // Show
        notificationText.text = message;
        notificationPanel.SetActive(true);

        // Fade in
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
        notificationPanel.SetActive(false);

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
