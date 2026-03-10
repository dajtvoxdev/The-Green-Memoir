using UnityEngine;

/// <summary>
/// Defines the type/category of an item for inventory and gameplay logic.
/// </summary>
public enum ItemType
{
    Seed,
    Crop,
    Tool,
    Consumable,
    Material
}

/// <summary>
/// ScriptableObject definition for items in Moonlit Garden.
/// Used to define item properties in the Unity Editor.
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "MoonlitGarden/Items/ItemDefinition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Unique identifier for this item (e.g., 'seed_tomato_001')")]
    public string itemId;
    
    [Tooltip("Display name shown in UI")]
    public string itemName;
    
    [Tooltip("Item description")]
    [TextArea]
    public string description;
    
    [Header("Type")]
    [Tooltip("Category/type of this item")]
    public ItemType itemType;
    
    [Tooltip("Icon sprite for UI display")]
    public Sprite icon;
    
    [Header("Stacking")]
    [Tooltip("Whether this item can stack in inventory")]
    public bool stackable;
    
    [Tooltip("Maximum stack size for this item")]
    public int maxStack = 99;
    
    [Header("Economy")]
    [Tooltip("Price to buy this item from shops")]
    public int buyPrice;
    
    [Tooltip("Price to sell this item to shops")]
    public int sellPrice;
    
    [Header("Crop Specific")]
    [Tooltip("If Seed type, links to the CropDefinition this seed grows into")]
    public string cropId;
    
    [Tooltip("Minimum yield when harvesting (for crops)")]
    public int yieldMin;
    
    [Tooltip("Maximum yield when harvesting (for crops)")]
    public int yieldMax;
}