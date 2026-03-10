using System;
using UnityEngine;

/// <summary>
/// ScriptableObject defining a shop's item catalog.
/// Create via: Assets > Create > MoonlitGarden > Shop > ShopCatalog
///
/// Phase 2 Feature (#21): Shop/Trading System.
/// </summary>
[CreateAssetMenu(fileName = "NewShopCatalog", menuName = "MoonlitGarden/Shop/ShopCatalog")]
public class ShopCatalog : ScriptableObject
{
    [Header("Shop Info")]
    public string shopName = "General Store";

    [TextArea]
    public string shopDescription;

    [Header("Items for Sale")]
    public ShopEntry[] entries;
}

/// <summary>
/// A single entry in a ShopCatalog — defines what can be bought/sold and at what price.
/// </summary>
[Serializable]
public class ShopEntry
{
    [Tooltip("Reference to the ItemDefinition")]
    public ItemDefinition item;

    [Tooltip("Price to buy from shop (0 = cannot buy)")]
    public int buyPrice;

    [Tooltip("Price shop pays when player sells (0 = cannot sell)")]
    public int sellPrice;

    [Tooltip("Whether this item is currently available")]
    public bool available = true;

    [Tooltip("Max stock (-1 = unlimited)")]
    public int maxStock = -1;

    [HideInInspector]
    public int currentStock;

    /// <summary>
    /// Whether this item can be bought.
    /// </summary>
    public bool CanBuy => available && buyPrice > 0 && (maxStock < 0 || currentStock > 0);

    /// <summary>
    /// Whether this item can be sold to this shop.
    /// </summary>
    public bool CanSell => available && sellPrice > 0;
}
