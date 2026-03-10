using UnityEngine;

/// <summary>
/// Base class for all UI panels (Inventory, Shop, Settings, etc.).
/// Attach to the root GameObject of each panel.
/// Provides Show/Hide with optional CanvasGroup fade animation.
///
/// Phase 2 Feature (#13): UI Framework foundation.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class PanelBase : MonoBehaviour
{
    [Header("Panel Settings")]
    [Tooltip("Unique identifier for this panel.")]
    public string panelId;

    [Tooltip("Should this panel pause the game when open?")]
    public bool pauseGameWhenOpen = false;

    [Tooltip("Should Escape key close this panel?")]
    public bool closeOnEscape = true;

    [Header("Animation")]
    [Tooltip("Fade speed for show/hide transitions. 0 = instant.")]
    public float fadeSpeed = 5f;

    /// <summary>
    /// Whether this panel is currently visible.
    /// </summary>
    public bool IsVisible { get; private set; }

    private CanvasGroup canvasGroup;
    private float targetAlpha;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Start hidden
        SetVisibleImmediate(false);
    }

    protected virtual void Update()
    {
        // Smooth fade animation
        if (fadeSpeed > 0f && !Mathf.Approximately(canvasGroup.alpha, targetAlpha))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);

            // When fully hidden, disable interaction
            if (canvasGroup.alpha <= 0.01f && targetAlpha == 0f)
            {
                SetInteractable(false);
                gameObject.SetActive(false);
            }
        }

        // Handle Escape to close
        if (closeOnEscape && IsVisible && Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }

    /// <summary>
    /// Shows the panel with fade animation.
    /// </summary>
    public virtual void Show()
    {
        gameObject.SetActive(true);
        IsVisible = true;
        targetAlpha = 1f;
        SetInteractable(true);

        if (fadeSpeed <= 0f)
        {
            canvasGroup.alpha = 1f;
        }

        if (pauseGameWhenOpen && GameManager.Instance != null)
        {
            GameManager.Instance.Pause();
        }

        OnShow();
    }

    /// <summary>
    /// Hides the panel with fade animation.
    /// </summary>
    public virtual void Hide()
    {
        IsVisible = false;
        targetAlpha = 0f;

        if (fadeSpeed <= 0f)
        {
            canvasGroup.alpha = 0f;
            SetInteractable(false);
            gameObject.SetActive(false);
        }

        if (pauseGameWhenOpen && GameManager.Instance != null)
        {
            GameManager.Instance.Resume();
        }

        OnHide();
    }

    /// <summary>
    /// Toggles visibility.
    /// </summary>
    public void Toggle()
    {
        if (IsVisible) Hide();
        else Show();
    }

    /// <summary>
    /// Sets visibility instantly without animation.
    /// </summary>
    public void SetVisibleImmediate(bool visible)
    {
        gameObject.SetActive(visible);
        IsVisible = visible;
        targetAlpha = visible ? 1f : 0f;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = targetAlpha;
            SetInteractable(visible);
        }
    }

    private void SetInteractable(bool interactable)
    {
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;
    }

    /// <summary>
    /// Called when panel becomes visible. Override for custom logic.
    /// </summary>
    protected virtual void OnShow() { }

    /// <summary>
    /// Called when panel is hidden. Override for custom logic.
    /// </summary>
    protected virtual void OnHide() { }
}
