using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Represents an item in the player's inventory.
/// Redesigned for Phase 1 with quantity support and itemId linking to ItemDefinition.
/// </summary>
public class InvenItems 
{
    /// <summary>
    /// Unique identifier linking to ItemDefinition ScriptableObject.
    /// Format: "seed_tomato_001", "crop_wheat_002", etc.
    /// </summary>
    public string itemId { get; set; }
    
    /// <summary>
    /// Display name (cached from ItemDefinition for convenience).
    /// </summary>
    public string name { get; set; }
    
    /// <summary>
    /// Description (cached from ItemDefinition for convenience).
    /// </summary>
    public string description { get; set; }
    
    /// <summary>
    /// Stack quantity of this item.
    /// For stackable items, this can be > 1.
    /// </summary>
    public int quantity { get; set; }
    
    /// <summary>
    /// Type of item: "Seed", "Crop", "Tool", "Consumable", "Material".
    /// Mirrors ItemType enum for JSON serialization.
    /// </summary>
    public string itemType { get; set; }
    
    /// <summary>
    /// Name of the icon sprite (for UI loading).
    /// </summary>
    public string iconName { get; set; }
    
    /// <summary>
    /// Default constructor for JSON serialization.
    /// </summary>
    public InvenItems()
    {
        itemId = string.Empty;
        name = string.Empty;
        description = string.Empty;
        quantity = 1;
        itemType = string.Empty;
        iconName = string.Empty;
    }

    /// <summary>
    /// Creates a new inventory item with basic info.
    /// </summary>
    public InvenItems(string name, string description)
    {
        this.itemId = string.Empty;
        this.name = name;
        this.description = description;
        this.quantity = 1;
        this.itemType = string.Empty;
        this.iconName = string.Empty;
    }
    
    /// <summary>
    /// Creates a new inventory item with full details.
    /// </summary>
    public InvenItems(string itemId, string name, string description, int quantity = 1, string itemType = "", string iconName = "")
    {
        this.itemId = itemId;
        this.name = name;
        this.description = description;
        this.quantity = quantity;
        this.itemType = itemType;
        this.iconName = iconName;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
