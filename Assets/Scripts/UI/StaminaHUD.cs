using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD element that shows the player's current stamina as a progress bar.
/// Dynamically changes fill color based on stamina level.
///
/// Phase 3 Feature (#33): Stamina HUD display.
///
/// Usage: Attach to a UI GameObject with a Slider child.
/// Wire staminaSlider, staminaText, and fillImage in the Inspector.
/// If fillImage is null, the bar still works — just without color feedback.
/// </summary>
public class StaminaHUD : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Slider showing current/max stamina fill.")]
    public Slider staminaSlider;

    [Tooltip("Text showing 'current/max' numbers.")]
    public TMP_Text staminaText;

    [Tooltip("The fill Image of the slider (for color changes).")]
    public Image fillImage;

    [Header("Colors")]
    [Tooltip("Fill color when stamina > 50%.")]
    public Color fullColor   = new Color(0.28f, 0.82f, 0.28f, 1f);

    [Tooltip("Fill color when stamina is 20–50%.")]
    public Color lowColor    = new Color(0.95f, 0.60f, 0.10f, 1f);

    [Tooltip("Fill color when stamina < 20%.")]
    public Color criticalColor = new Color(0.88f, 0.20f, 0.20f, 1f);

    void Start()
    {
        AutoWireReferences();

        if (StaminaManager.Instance != null)
        {
            StaminaManager.Instance.OnStaminaChanged += UpdateDisplay;
            UpdateDisplay(StaminaManager.Instance.CurrentStamina, StaminaManager.Instance.maxStamina);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Auto-finds child UI elements if Inspector references are missing.
    /// </summary>
    private void AutoWireReferences()
    {
        if (staminaSlider == null)
        {
            staminaSlider = GetComponentInChildren<Slider>();
        }

        if (staminaText == null)
        {
            var textT = transform.Find("StaminaText");
            if (textT != null) staminaText = textT.GetComponent<TMP_Text>();
        }

        if (fillImage == null && staminaSlider != null && staminaSlider.fillRect != null)
        {
            fillImage = staminaSlider.fillRect.GetComponent<Image>();
        }
    }

    private void UpdateDisplay(int current, int max)
    {
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = max;
            staminaSlider.value    = current;
        }

        if (staminaText != null)
        {
            staminaText.text = $"{current}/{max}";
        }

        if (fillImage != null)
        {
            float ratio = max > 0 ? (float)current / max : 0f;
            fillImage.color = ratio > 0.5f ? fullColor
                            : ratio > 0.2f ? lowColor
                            : criticalColor;
        }
    }

    void OnDestroy()
    {
        if (StaminaManager.Instance != null)
        {
            StaminaManager.Instance.OnStaminaChanged -= UpdateDisplay;
        }
    }
}
