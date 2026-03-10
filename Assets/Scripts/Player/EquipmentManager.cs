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

    void Start()
    {
        // Equip the first starter tool by default (typically Hoe)
        if (currentTool == null && starterTools != null && starterTools.Length > 0)
        {
            EquipTool(starterTools[0]);
        }
    }

    void Update()
    {
        HandleToolSwitchInput();
    }

    /// <summary>
    /// Handles number key input for quick tool switching.
    /// Keys 1-6 map to starterTools array indices.
    /// </summary>
    private void HandleToolSwitchInput()
    {
        if (starterTools == null) return;

        for (int i = 0; i < Mathf.Min(starterTools.Length, 6); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                EquipTool(starterTools[i]);
                break;
            }
        }
    }

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
            NotificationManager.Instance?.ShowNotification($"Equipped: {tool.toolName}", 1.5f);
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
