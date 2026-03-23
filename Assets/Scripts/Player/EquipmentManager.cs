using System;
using UnityEngine;

/// <summary>
/// Manages the player's currently equipped tool.
/// Farm actions route through the equipped tool to determine behavior.
///
/// Phase 2 Feature (#26): Item use & tool equip.
///
/// Usage:
///   EquipmentManager.Instance.EquipTool(hoeDefinition);
///   ToolType? current = EquipmentManager.Instance.CurrentToolType;
///   EquipmentManager.Instance.UnequipTool();
/// </summary>
public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    [Header("Default Tools")]
    [Tooltip("Tools available from the start (basic set). Drag ToolDefinition assets here.")]
    public ToolDefinition[] starterTools;

    [Header("Current State")]
    [SerializeField] private ToolDefinition currentTool;

    /// <summary>
    /// The currently equipped tool definition (null if nothing equipped).
    /// </summary>
    public ToolDefinition CurrentTool => currentTool;

    /// <summary>
    /// The current tool type (null if nothing equipped).
    /// </summary>
    public ToolType? CurrentToolType => currentTool != null ? currentTool.toolType : (ToolType?)null;

    /// <summary>
    /// If current tool is a SeedBag, returns the cropId to plant.
    /// </summary>
    public string CurrentSeedCropId =>
        currentTool != null && currentTool.toolType == ToolType.SeedBag ? currentTool.cropId : null;

    /// <summary>
    /// Fired when the equipped tool changes. Parameter: new ToolDefinition (null if unequipped).
    /// </summary>
    public event Action<ToolDefinition> OnToolChanged;

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

    // Farming is context-sensitive (right-click auto-detects action).
    // No need to auto-equip a tool at startup.

    // Keys 1-9 are reserved for seed selection (SeedQuickbarUI).
    // Tools are selected by clicking the quickbar UI slots.

    /// <summary>
    /// Equips a tool.
    /// </summary>
    public void EquipTool(ToolDefinition tool)
    {
        if (tool == currentTool) return;

        currentTool = tool;
        OnToolChanged?.Invoke(currentTool);

        if (tool != null)
        {
            Debug.Log($"Equipment: Equipped '{tool.toolName}' ({tool.toolType})");
            AudioManager.Instance?.PlaySFX("equip");
            NotificationManager.Instance?.ShowNotification($"Đã trang bị: {tool.toolName}", 1.5f);
        }
    }

    /// <summary>
    /// Unequips the current tool.
    /// </summary>
    public void UnequipTool()
    {
        if (currentTool == null) return;

        Debug.Log("Equipment: Unequipped tool");
        currentTool = null;
        OnToolChanged?.Invoke(null);
    }

    /// <summary>
    /// Gets a starter tool by ToolType.
    /// </summary>
    public ToolDefinition GetStarterTool(ToolType type)
    {
        if (starterTools == null) return null;

        foreach (var tool in starterTools)
        {
            if (tool != null && tool.toolType == type)
            {
                return tool;
            }
        }
        return null;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
