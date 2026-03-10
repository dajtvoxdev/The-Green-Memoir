using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// HUD element displaying the currently equipped tool.
/// Subscribes to EquipmentManager.OnToolChanged for live updates.
///
/// Phase 2 Feature (#26): Tool display in HUD.
/// </summary>
public class ToolHUD : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text showing current tool name.")]
    public TMP_Text toolNameText;

    [Tooltip("Image showing current tool icon (optional).")]
    public Image toolIcon;

    void Start()
    {
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnToolChanged += OnToolChanged;
            OnToolChanged(EquipmentManager.Instance.CurrentTool);
        }
    }

    private void OnToolChanged(ToolDefinition tool)
    {
        if (toolNameText != null)
        {
            toolNameText.text = tool != null
                ? $"[{tool.toolName}]"
                : LocalizationManager.LocalizeText("[No Tool]");
        }

        if (toolIcon != null)
        {
            if (tool != null && tool.icon != null)
            {
                toolIcon.sprite = tool.icon;
                toolIcon.enabled = true;
            }
            else
            {
                toolIcon.enabled = false;
            }
        }
    }

    void OnDestroy()
    {
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnToolChanged -= OnToolChanged;
        }
    }
}
