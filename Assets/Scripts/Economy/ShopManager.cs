using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages shop buy/sell transactions.
/// Validates Gold, inventory capacity, and stock before completing trades.
///
/// Phase 2 Feature (#21): Shop/Trading System.
///
/// Usage:
///   ShopManager.Instance.OpenShop(generalStoreCatalog);
///   ShopManager.Instance.BuyItem(shopEntry, 1);
///   ShopManager.Instance.SellItem(inventoryItem, 1);
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    /// <summary>
    /// Currently active shop catalog (null if no shop is open).
    /// </summary>
    public ShopCatalog CurrentCatalog { get; private set; }

    /// <summary>
    /// Whether a shop is currently open.
    /// </summary>
    public bool IsShopOpen => CurrentCatalog != null;

    [Header("References")]
    [Tooltip("Reference to inventory manager for adding/removing items.")]
    public RecyclableInventoryManager inventoryManager;

    /// <summary>
    /// Fired when a shop is opened. Parameter: ShopCatalog.
    /// </summary>
    public event Action<ShopCatalog> OnShopOpened;

    /// <summary>
    /// Fired when the shop is closed.
    /// </summary>
    public event Action OnShopClosed;

    /// <summary>
    /// Fired when an item is bought. Parameters: ShopEntry, quantity.
    /// </summary>
    public event Action<ShopEntry, int> OnItemBought;

    /// <summary>
    /// Fired when an item is sold. Parameters: itemId, quantity, totalGold.
    /// </summary>
    public event Action<string, int, int> OnItemSold;

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
        if (inventoryManager == null)
        {
            inventoryManager = FindAnyObjectByType<RecyclableInventoryManager>();
        }
    }

    // ==================== SHOP LIFECYCLE ====================

    /// <summary>
    /// Opens a shop with the given catalog.
    /// </summary>
    public void OpenShop(ShopCatalog catalog)
    {
        if (catalog == null)
        {
            Debug.LogWarning("ShopManager: Cannot open shop — catalog is null.");
            return;
        }

        CurrentCatalog = catalog;

        // Reset stock for limited items
        foreach (var entry in catalog.entries)
        {
            if (entry.maxStock > 0)
            {
                entry.currentStock = entry.maxStock;
            }
        }

        OnShopOpened?.Invoke(catalog);
        Debug.Log($"ShopManager: Opened '{catalog.shopName}' with {catalog.entries.Length} items.");
    }

    /// <summary>
    /// Closes the current shop.
    /// </summary>
    public void CloseShop()
    {
        CurrentCatalog = null;
        OnShopClosed?.Invoke();
        Debug.Log("ShopManager: Shop closed.");
    }

    // ==================== BUY ====================

    /// <summary>
    /// Attempts to buy an item from the shop.
    /// </summary>
    /// <param name="entry">The ShopEntry to buy</param>
    /// <param name="quantity">How many to buy</param>
    /// <returns>True if purchase succeeded</returns>
    public bool BuyItem(ShopEntry entry, int quantity = 1)
    {
        if (entry == null || quantity <= 0) return false;

        if (!entry.CanBuy)
        {
            ShowError("Vật phẩm này không thể mua.");
            return false;
        }

        // Check stock
        if (entry.maxStock > 0 && entry.currentStock < quantity)
        {
            ShowError($"Hết hàng! Chỉ còn {entry.currentStock}.");
            return false;
        }

        int totalCost = entry.buyPrice * quantity;

        // Check Gold
        if (PlayerEconomyManager.Instance == null || !PlayerEconomyManager.Instance.CanAffordGold(totalCost))
        {
            ShowError($"Không đủ vàng! Cần {totalCost}G.");
            return false;
        }

        // Check inventory space
        InvenItems newItem = CreateItemFromEntry(entry, quantity);
        if (inventoryManager != null && !inventoryManager.CanAddItem(newItem))
        {
            ShowError("Túi đồ đã đầy!");
            return false;
        }

        // Execute transaction
        PlayerEconomyManager.Instance.SpendGold(totalCost);

        if (inventoryManager != null)
        {
            inventoryManager.AddInventoryItem(newItem);
            inventoryManager.FlushSave();
        }

        // Reduce stock
        if (entry.maxStock > 0)
        {
            entry.currentStock -= quantity;
        }

        OnItemBought?.Invoke(entry, quantity);
        AudioManager.Instance?.PlaySFX("buy");

        string itemName = entry.item.itemName;
        string msg = $"Đã mua {quantity}x {itemName} với giá {totalCost}G";
        Debug.Log($"ShopManager: {msg}");
        NotificationManager.Instance?.ShowNotification(msg);

        return true;
    }

    // ==================== SELL ====================

    /// <summary>
    /// Attempts to sell an item from inventory to the shop.
    /// </summary>
    /// <param name="itemId">The itemId to sell</param>
    /// <param name="quantity">How many to sell</param>
    /// <returns>True if sale succeeded</returns>
    public bool SellItem(string itemId, int quantity = 1)
    {
        if (string.IsNullOrEmpty(itemId) || quantity <= 0) return false;

        if (CurrentCatalog == null)
        {
            ShowError("Chưa mở cửa hàng!");
            return false;
        }

        // Find the sell price — try shop catalog first, then CropDefinition fallback
        ShopEntry sellEntry = FindEntryByItemId(itemId);
        int sellPrice = 0;
        string itemName = itemId;

        if (sellEntry != null && sellEntry.CanSell)
        {
            sellPrice = sellEntry.sellPrice;
            itemName = sellEntry.item != null ? sellEntry.item.itemName : itemId;
        }
        else if (itemId.StartsWith("crop_") && CropGrowthManager.Instance != null)
        {
            // Harvested crop — look up sell price from CropDefinition
            string cropId = itemId.Substring(5);
            var cropDef = CropGrowthManager.Instance.GetCropDefinition(cropId);
            if (cropDef != null && cropDef.sellPrice > 0)
            {
                sellPrice = cropDef.sellPrice;
                itemName = cropDef.cropName;
            }
        }

        if (sellPrice <= 0)
        {
            ShowError("Cửa hàng không thu mua vật phẩm này.");
            return false;
        }

        // Check inventory has enough
        if (inventoryManager == null) return false;

        List<InvenItems> items = inventoryManager.GetInventoryItems();
        int foundIndex = -1;
        int availableQuantity = 0;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemId == itemId)
            {
                foundIndex = i;
                availableQuantity = items[i].quantity;
                break;
            }
        }

        if (foundIndex < 0 || availableQuantity < quantity)
        {
            ShowError($"Bạn không có đủ {quantity}x vật phẩm này.");
            return false;
        }

        int totalGold = sellPrice * quantity;

        // Execute transaction
        inventoryManager.RemoveQuantityAt(foundIndex, quantity);
        inventoryManager.FlushSave();
        PlayerEconomyManager.Instance?.EarnGold(totalGold);

        OnItemSold?.Invoke(itemId, quantity, totalGold);
        AudioManager.Instance?.PlaySFX("sell");

        string msg = $"Đã bán {quantity}x {itemName} được {totalGold}G";
        Debug.Log($"ShopManager: {msg}");
        NotificationManager.Instance?.ShowNotification(msg);

        return true;
    }

    // ==================== HELPERS ====================

    /// <summary>
    /// Finds a ShopEntry by itemId in the current catalog.
    /// </summary>
    public ShopEntry FindEntryByItemId(string itemId)
    {
        if (CurrentCatalog?.entries == null) return null;

        foreach (var entry in CurrentCatalog.entries)
        {
            if (entry.item != null && entry.item.itemId == itemId)
            {
                return entry;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets all available entries from the current catalog.
    /// </summary>
    public List<ShopEntry> GetAvailableEntries()
    {
        var result = new List<ShopEntry>();
        if (CurrentCatalog?.entries == null) return result;

        foreach (var entry in CurrentCatalog.entries)
        {
            if (entry.available && entry.item != null)
            {
                result.Add(entry);
            }
        }
        return result;
    }

    /// <summary>
    /// Creates an InvenItems from a ShopEntry for adding to inventory.
    /// </summary>
    private InvenItems CreateItemFromEntry(ShopEntry entry, int quantity)
    {
        var item = entry.item;
        return new InvenItems(
            itemId: item.itemId,
            name: item.itemName,
            description: item.description,
            quantity: quantity,
            itemType: item.itemType.ToString(),
            iconName: item.icon != null ? item.icon.name : ""
        );
    }

    private void ShowError(string message)
    {
        Debug.LogWarning($"ShopManager: {message}");
        NotificationManager.Instance?.ShowNotification(message);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
