using UnityEngine;

/// <summary>
/// Types of tools in Moonlit Garden.
/// Each tool type maps to a specific farm action.
/// </summary>
public enum ToolType
{
    Hoe,            // Till ground
    WateringCan,    // Water crops
    Sickle,         // Harvest crops
    SeedBag,        // Plant seeds (requires cropId)
    Axe,            // Chop trees (future)
    Pickaxe         // Break rocks (future)
}

/// <summary>
/// ScriptableObject defining a tool's properties.
/// Create via: Assets > Create > MoonlitGarden > Items > ToolDefinition
///
/// Phase 2 Feature (#26): Tool/Equipment system.
/// </summary>
[CreateAssetMenu(fileName = "NewTool", menuName = "MoonlitGarden/Items/ToolDefinition")]
public class ToolDefinition : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique tool ID (e.g., 'tool_hoe_basic')")]
    public string toolId;

    [Tooltip("Display name")]
    public string toolName;

    [TextArea]
    public string description;

    [Header("Tool Properties")]
    [Tooltip("What type of tool this is")]
    public ToolType toolType;

    [Tooltip("Tool tier (higher = better stats)")]
    public int tier = 1;

    [Header("Visuals")]
    [Tooltip("Icon sprite for UI display")]
    public Sprite icon;

    [Header("Economy")]
    [Tooltip("Buy price from shop")]
    public int buyPrice;

    [Tooltip("Sell price to shop")]
    public int sellPrice;

    [Header("Seed-specific (only for SeedBag type)")]
    [Tooltip("If SeedBag, which crop this plants")]
    public string cropId;
}
