using PolyAndCode.UI;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the recyclable inventory scroll view with item stacking support.
/// Phase 1 Feature (#16): Item stacking system with quantity management.
/// </summary>
public class RecyclableInventoryManager : MonoBehaviour, IRecyclableScrollRectDataSource
{
    [SerializeField]
    RecyclableScrollRect _recyclableScrollRect;

    [SerializeField]
    private int _dataLength;

    public GameObject inventoryGameObject;
    
    [Header("Inventory Settings")]
    [Tooltip("Maximum number of inventory slots")]
    public int maxInventorySlots = 20;

    // Inventory data with stacking support
    private List<InvenItems> _invenItems = new List<InvenItems>();
    
    // Cache of ItemDefinitions for quick lookup
    private Dictionary<string, ItemDefinition> _itemDefinitionCache = new Dictionary<string, ItemDefinition>();
    
    // Default max stack size for items without ItemDefinition
    private const int DefaultMaxStack = 99;

    //Recyclable scroll rect's data source must be assigned in Awake.
    private void Awake()
    {
        _recyclableScrollRect.DataSource = this;
    }
    
    /// <summary>
    /// Data source method. Returns the list length.
    /// </summary>
    public int GetItemCount()
    {
        return _invenItems.Count;
    }

    /// <summary>
    /// Called for a cell every time it is recycled.
    /// Implement this method to do the necessary cell configuration.
    /// </summary>
    public void SetCell(ICell cell, int index)
    {
        //Casting to the implemented Cell
        var item = cell as CelllItemData;
        if (index < _invenItems.Count)
        {
            item.ConfigureCell(_invenItems[index], index);
        }
    }

    private void Start()
    {
        // Phase 1: Removed dummy data generation
        // Inventory should be loaded from Firebase/LoadDataManager instead
        Debug.Log("RecyclableInventoryManager: Initialized with empty inventory");
    }
    
    /// <summary>
    /// Loads inventory items from a list (called from LoadDataManager after Firebase load).
    /// </summary>
    public void SetLstItem(List<InvenItems> lst)
    {
        _invenItems = lst ?? new List<InvenItems>();
        _recyclableScrollRect?.ReloadData();
    }
    
    /// <summary>
    /// Gets the current inventory items list.
    /// </summary>
    public List<InvenItems> GetInventoryItems()
    {
        return new List<InvenItems>(_invenItems);
    }

    private void Update()
    {
        // Phase 1: Fixed inventory toggle to use SetActive instead of Y position hack
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleInventory();
        }
        
        // Debug: Add test item with L key
        if (Input.GetKeyDown(KeyCode.L))
        {
            AddTestItem();
        }
    }
    
    /// <summary>
    /// Toggles the inventory UI visibility.
    /// Phase 1: Uses SetActive instead of Y position hack.
    /// </summary>
    public void ToggleInventory()
    {
        if (inventoryGameObject != null)
        {
            inventoryGameObject.SetActive(!inventoryGameObject.activeSelf);
        }
    }
    
    /// <summary>
    /// Shows the inventory UI.
    /// </summary>
    public void ShowInventory()
    {
        if (inventoryGameObject != null)
        {
            inventoryGameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hides the inventory UI.
    /// </summary>
    public void HideInventory()
    {
        if (inventoryGameObject != null)
        {
            inventoryGameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Adds an item to the inventory with stacking support.
    /// Phase 1: Implements item stacking - identical items stack together.
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <returns>True if item was added successfully, false if inventory is full</returns>
    public bool AddInventoryItem(InvenItems item)
    {
        if (item == null)
        {
            Debug.LogError("RecyclableInventoryManager: Cannot add null item");
            return false;
        }
        
        // Get max stack size for this item
        int maxStack = GetMaxStackSize(item);
        
        // Try to stack with existing items first
        foreach (var existingItem in _invenItems)
        {
            // Stack if itemIds match (or both have same name for backward compatibility)
            if (ShouldStack(existingItem, item))
            {
                int spaceRemaining = maxStack - existingItem.quantity;
                if (spaceRemaining > 0)
                {
                    // Add as much as possible to existing stack
                    int amountToAdd = Mathf.Min(item.quantity, spaceRemaining);
                    existingItem.quantity += amountToAdd;
                    item.quantity -= amountToAdd;
                    
                    Debug.Log($"RecyclableInventoryManager: Stacked {amountToAdd} {item.name} (total: {existingItem.quantity})");
                    
                    // If all items were stacked, we're done
                    if (item.quantity <= 0)
                    {
                        _recyclableScrollRect?.ReloadData();
                        return true;
                    }
                }
            }
        }
        
        // If we still have items left, create a new stack
        if (_invenItems.Count >= maxInventorySlots)
        {
            Debug.LogWarning($"RecyclableInventoryManager: Inventory is full ({_invenItems.Count}/{maxInventorySlots})");
            return false;
        }
        
        _invenItems.Add(item);
        Debug.Log($"RecyclableInventoryManager: Added new stack of {item.quantity} {item.name}");
        _recyclableScrollRect?.ReloadData();
        return true;
    }
    
    /// <summary>
    /// Adds multiple items to the inventory.
    /// </summary>
    public bool AddInventoryItems(List<InvenItems> items)
    {
        bool allAdded = true;
        foreach (var item in items)
        {
            if (!AddInventoryItem(item))
            {
                allAdded = false;
            }
        }
        return allAdded;
    }
    
    /// <summary>
    /// Removes an item from the inventory by index.
    /// </summary>
    /// <param name="index">The index to remove</param>
    /// <returns>True if removed successfully</returns>
    public bool RemoveItemAt(int index)
    {
        if (index >= 0 && index < _invenItems.Count)
        {
            _invenItems.RemoveAt(index);
            _recyclableScrollRect?.ReloadData();
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Removes a quantity of items from a stack at the given index.
    /// </summary>
    /// <param name="index">The stack index</param>
    /// <param name="quantity">Quantity to remove</param>
    /// <returns>True if removed successfully</returns>
    public bool RemoveQuantityAt(int index, int quantity)
    {
        if (index >= 0 && index < _invenItems.Count)
        {
            var item = _invenItems[index];
            item.quantity -= quantity;
            
            if (item.quantity <= 0)
            {
                _invenItems.RemoveAt(index);
            }
            
            _recyclableScrollRect?.ReloadData();
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Clears all items from the inventory.
    /// </summary>
    public void ClearInventory()
    {
        _invenItems.Clear();
        _recyclableScrollRect?.ReloadData();
    }
    
    /// <summary>
    /// Checks if an item can be added to the inventory.
    /// </summary>
    public bool CanAddItem(InvenItems item)
    {
        if (item == null) return false;
        
        // Check if we can stack with existing items
        foreach (var existingItem in _invenItems)
        {
            if (ShouldStack(existingItem, item))
            {
                int maxStack = GetMaxStackSize(item);
                if (existingItem.quantity + item.quantity <= maxStack)
                {
                    return true;
                }
            }
        }
        
        // Check if we have empty slots
        return _invenItems.Count < maxInventorySlots;
    }
    
    /// <summary>
    /// Determines if two items should stack together.
    /// </summary>
    private bool ShouldStack(InvenItems existing, InvenItems newItem)
    {
        // Stack if itemIds match (primary check)
        if (!string.IsNullOrEmpty(existing.itemId) && !string.IsNullOrEmpty(newItem.itemId))
        {
            return existing.itemId == newItem.itemId;
        }
        
        // Fallback to name matching for backward compatibility
        return existing.name == newItem.name;
    }
    
    /// <summary>
    /// Gets the max stack size for an item.
    /// </summary>
    private int GetMaxStackSize(InvenItems item)
    {
        // Try to get from ItemDefinition cache
        if (!string.IsNullOrEmpty(item.itemId))
        {
            if (!_itemDefinitionCache.TryGetValue(item.itemId, out ItemDefinition def))
            {
                // Try to find in Resources (ItemDefinitions should be in Resources/Items folder)
                var definitions = Resources.LoadAll<ItemDefinition>("Items");
                foreach (var definition in definitions)
                {
                    if (definition.itemId == item.itemId)
                    {
                        _itemDefinitionCache[item.itemId] = definition;
                        def = definition;
                        break;
                    }
                }
            }
            
            if (def != null)
            {
                return def.stackable ? def.maxStack : 1;
            }
        }
        
        // Default max stack
        return DefaultMaxStack;
    }
    
    /// <summary>
    /// Adds a test item for debugging (L key).
    /// </summary>
    private void AddTestItem()
    {
        InvenItems testItem = new InvenItems(
            itemId: "test_item_001",
            name: "Test Item",
            description: "A test item for debugging",
            quantity: 1,
            itemType: "Material",
            iconName: ""
        );
        
        if (AddInventoryItem(testItem))
        {
            Debug.Log("RecyclableInventoryManager: Added test item");
        }
        else
        {
            Debug.LogWarning("RecyclableInventoryManager: Could not add test item (inventory full?)");
        }
    }
}