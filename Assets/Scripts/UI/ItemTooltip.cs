using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Tooltip popup that shows detailed item information.
/// Follows a singleton pattern — only one tooltip visible at a time.
///
/// Phase 2 Feature (#28): Inventory item tooltip.
/// </summary>
public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance { get; private set; }

    [Header("UI References")]
    public TMP_Text itemNameText;
    public TMP_Text itemDescText;
    public TMP_Text itemStatsText;

    [Header("Settings")]
    [Tooltip("Offset from mouse/touch position.")]
    public Vector2 offset = new Vector2(10, -10);

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas parentCanvas;

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

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        EnsureVisualStyle();

        Hide();
    }

    /// <summary>
    /// Shows the tooltip with item information.
    /// </summary>
    public void Show(InvenItems item, Vector2 screenPosition)
    {
        if (item == null) return;

        if (itemNameText != null)
        {
            itemNameText.text = item.name;
        }

        if (itemDescText != null)
        {
            itemDescText.text = !string.IsNullOrEmpty(item.description)
                ? item.description
                : LocalizationManager.LocalizeText("No description.");
        }

        if (itemStatsText != null)
        {
            string stats = $"Qty: {item.quantity}";
            if (!string.IsNullOrEmpty(item.itemType))
            {
                stats += $"  |  Type: {item.itemType}";
            }
            itemStatsText.text = LocalizationManager.LocalizeText(stats);
        }

        // Position near the cursor/touch
        if (rectTransform != null && parentCanvas != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                screenPosition + offset,
                parentCanvas.worldCamera,
                out localPoint
            );
            rectTransform.localPosition = localPoint;

            ClampToScreen();
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    /// <summary>
    /// Hides the tooltip.
    /// </summary>
    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// Clamps tooltip position so it stays within screen bounds.
    /// </summary>
    private void ClampToScreen()
    {
        if (rectTransform == null || parentCanvas == null) return;

        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        RectTransform canvasRT = parentCanvas.transform as RectTransform;
        Rect canvasRect = canvasRT.rect;

        Vector3 pos = rectTransform.localPosition;

        // Convert corners to canvas local space for comparison
        for (int i = 0; i < 4; i++)
        {
            corners[i] = canvasRT.InverseTransformPoint(corners[i]);
        }

        float minX = corners[0].x;
        float maxX = corners[2].x;
        float minY = corners[0].y;
        float maxY = corners[2].y;

        if (maxX > canvasRect.xMax) pos.x -= (maxX - canvasRect.xMax);
        if (minX < canvasRect.xMin) pos.x += (canvasRect.xMin - minX);
        if (maxY > canvasRect.yMax) pos.y -= (maxY - canvasRect.yMax);
        if (minY < canvasRect.yMin) pos.y += (canvasRect.yMin - minY);

        rectTransform.localPosition = pos;
    }

    private void EnsureVisualStyle()
    {
        if (rectTransform != null)
        {
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.sizeDelta = new Vector2(
                Mathf.Max(rectTransform.sizeDelta.x, 220f),
                Mathf.Max(rectTransform.sizeDelta.y, 120f)
            );
        }

        Image bg = GetComponent<Image>();
        if (bg == null)
        {
            bg = gameObject.AddComponent<Image>();
        }
        bg.color = new Color(0.06f, 0.04f, 0.02f, 0.96f);

        Outline outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }
        outline.effectColor = new Color(0.83f, 0.66f, 0.24f, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
