using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// A single slot in the Quickbar UI.
/// Displays tool icon, key binding number, and selected highlight.
/// Click to equip the assigned tool.
///
/// Phase 2 Feature (#27): Quickbar visual slot.
/// </summary>
public class QuickbarSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [Tooltip("Image showing the tool icon.")]
    public Image iconImage;

    [Tooltip("Text showing the key binding number (1-6).")]
    public TMP_Text keyText;

    [Tooltip("Text showing the tool name (displayed below icon).")]
    public TMP_Text nameText;

    [Tooltip("Background image for highlight/selection state.")]
    public Image backgroundImage;

    [Header("Colors")]
    public Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);
    public Color selectedColor = new Color(0.4f, 0.7f, 0.3f, 0.9f);
    public Color emptyColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);

    /// <summary>
    /// The ToolDefinition currently assigned to this slot (null if empty).
    /// </summary>
    public ToolDefinition AssignedTool { get; private set; }

    /// <summary>
    /// The slot index (0-based).
    /// </summary>
    public int SlotIndex { get; private set; }

    /// <summary>
    /// Whether this slot is currently selected/active.
    /// </summary>
    public bool IsSelected { get; private set; }

    /// <summary>
    /// Configures the slot with a tool and key binding index.
    /// </summary>
    /// <param name="tool">Tool to display (null for empty slot).</param>
    /// <param name="index">Slot index (0-based, displayed as index+1).</param>
    public void Setup(ToolDefinition tool, int index)
    {
        SlotIndex = index;
        AssignedTool = tool;

        // Key number label
        if (keyText != null)
        {
            keyText.text = (index + 1).ToString();
        }

        if (tool != null)
        {
            // Populated slot
            if (nameText != null)
            {
                nameText.text = tool.toolName;
            }

            if (iconImage != null)
            {
                if (tool.icon != null)
                {
                    iconImage.sprite = tool.icon;
                    iconImage.color = Color.white;
                    iconImage.enabled = true;
                }
                else
                {
                    // No icon — show tool type initial as placeholder
                    iconImage.enabled = false;
                }
            }

            SetBackgroundColor(IsSelected ? selectedColor : normalColor);
        }
        else
        {
            // Empty slot
            if (nameText != null)
            {
                nameText.text = "";
            }

            if (iconImage != null)
            {
                iconImage.enabled = false;
            }

            SetBackgroundColor(emptyColor);
        }
    }

    /// <summary>
    /// Sets the selected state of this slot.
    /// </summary>
    public void SetSelected(bool selected)
    {
        IsSelected = selected;

        if (AssignedTool != null)
        {
            SetBackgroundColor(selected ? selectedColor : normalColor);
        }
        else
        {
            SetBackgroundColor(emptyColor);
        }
    }

    /// <summary>
    /// Handles click on this slot to equip the assigned tool.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (AssignedTool != null && EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.EquipTool(AssignedTool);
        }
    }

    private void SetBackgroundColor(Color color)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = color;
        }
    }
}
