using PolyAndCode.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Inventory cell that displays item info and handles click/hover interactions.
/// Phase 2 Enhancement (#28): Added click-to-select, tooltip on hover, quantity display.
/// </summary>
public class CelllItemData : MonoBehaviour, ICell, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    public Text nameLabel;
    public Text desLabel;
    public TMP_Text quantityLabel;
    public Image iconImage;
    public Image highlightImage;

    // Model
    private InvenItems _contactInfo;
    private int _cellIndex;

    /// <summary>
    /// Called from the SetCell method in DataSource.
    /// Configures display for the given inventory item.
    /// </summary>
    public void ConfigureCell(InvenItems invenItems, int cellIndex)
    {
        _cellIndex = cellIndex;
        _contactInfo = invenItems;

        if (nameLabel != null)
        {
            nameLabel.text = invenItems.name;
        }

        if (desLabel != null)
        {
            desLabel.text = invenItems.description;
        }

        // Show quantity badge for stacked items
        if (quantityLabel != null)
        {
            if (invenItems.quantity > 1)
            {
                quantityLabel.text = invenItems.quantity.ToString();
                quantityLabel.gameObject.SetActive(true);
            }
            else
            {
                quantityLabel.gameObject.SetActive(false);
            }
        }

        // Deselect visual on reconfigure
        SetHighlight(false);
    }

    /// <summary>
    /// Handles click — opens the inventory action panel for this item.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_contactInfo == null) return;

        // Show action panel
        if (InventoryActionPanel.Instance != null)
        {
            InventoryActionPanel.Instance.Show(_contactInfo, _cellIndex);
        }

        // Show tooltip at click position
        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.Show(_contactInfo, eventData.position);
        }

        SetHighlight(true);
        AudioManager.Instance?.PlaySFX("ui_click");
    }

    /// <summary>
    /// Handles pointer enter — shows tooltip.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_contactInfo == null) return;

        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.Show(_contactInfo, eventData.position);
        }
    }

    /// <summary>
    /// Handles pointer exit — hides tooltip.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.Hide();
        }
    }

    private void SetHighlight(bool active)
    {
        if (highlightImage != null)
        {
            highlightImage.enabled = active;
        }
    }
}
