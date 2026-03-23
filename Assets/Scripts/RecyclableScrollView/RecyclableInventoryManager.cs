using PolyAndCode.UI;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the recyclable inventory scroll view with item stacking support.
/// Phase 1 Feature (#16): Item stacking system with quantity management.
/// Phase 2.5E Fix (BP3): Firebase persistence for inventory data.
/// </summary>
public class RecyclableInventoryManager : MonoBehaviour, IRecyclableScrollRectDataSource
{
    /// <summary>
    /// Fired whenever inventory contents change (add/remove/quantity update).
    /// Used by SeedQuickbarUI to refresh seed slots.
    /// </summary>
    public event Action OnInventoryChanged;
    [SerializeField]
    RecyclableScrollRect _recyclableScrollRect;

    [SerializeField]
    private int _dataLength;

    public GameObject inventoryGameObject;

    [Header("Inventory Layout")]
    [Tooltip("Number of columns shown in the inventory grid.")]
    [SerializeField] private int inventoryColumns = 4;

    // CanvasGroup used to show/hide inventory without deactivating the GameObject
    // (deactivating stops the RecyclableScrollRect coroutine)
    private CanvasGroup _inventoryCanvasGroup;

    [Header("Inventory Settings")]
    [Tooltip("Maximum number of inventory slots")]
    public int maxInventorySlots = 20;

    // Inventory data with stacking support
    private List<InvenItems> _invenItems = new List<InvenItems>();

    // Cache of ItemDefinitions for quick lookup
    private Dictionary<string, ItemDefinition> _itemDefinitionCache = new Dictionary<string, ItemDefinition>();

    // Default max stack size for items without ItemDefinition
    private const int DefaultMaxStack = 99;

    // Phase 2.5E: Batched save to prevent rapid Firebase writes
    private float pendingSaveTimer;
    private bool hasPendingSave;
    private const float SAVE_BATCH_DELAY = 1.5f;

    // Deferred reload: prevents Canvas rebuild conflicts when modifying UI hierarchy
    private bool _reloadPending;
    private Coroutine _reloadCoroutine;

    // Recyclable scroll rect's data source must be assigned in Awake.
    // Initialize() is also called here so the RecyclableScroll GO is guaranteed active.
    private void Awake()
    {
        if (_recyclableScrollRect == null)
        {
            Debug.LogError("RecyclableInventoryManager: _recyclableScrollRect is not assigned! Please assign it in the Inspector.");
            return;
        }

        _recyclableScrollRect.Direction = RecyclableScrollRect.DirectionType.Vertical;
        _recyclableScrollRect.IsGrid = true;
        _recyclableScrollRect.Segments = Mathf.Max(2, inventoryColumns);
        _recyclableScrollRect.DataSource = this;

        // Set up CanvasGroup-based visibility (keeps GO active for coroutines)
        if (inventoryGameObject != null)
        {
            _inventoryCanvasGroup = inventoryGameObject.GetComponent<CanvasGroup>();
            if (_inventoryCanvasGroup == null)
                _inventoryCanvasGroup = inventoryGameObject.AddComponent<CanvasGroup>();
            SetInventoryVisible(false);
        }

        // Defer Initialize to avoid canvas rebuild conflicts during scene load.
        StartCoroutine(DeferredInitialize());
    }

    private System.Collections.IEnumerator DeferredInitialize()
    {
        yield return new WaitForEndOfFrame();

        // Remove VerticalLayoutGroup and ContentSizeFitter from scroll content
        // if present.  The RecyclableScrollRect library manages cell layout
        // manually via anchoredPosition and Content.sizeDelta.  Having Unity's
        // layout system on the same Content causes cascading
        // OnRectTransformDimensionsChange → SetAllDirty →
        // RegisterCanvasElementForGraphicRebuild during the Canvas graphic
        // rebuild pass, producing "graphic rebuild loop" errors.
        if (_recyclableScrollRect != null && _recyclableScrollRect.content != null)
        {
            var scrollContent = _recyclableScrollRect.content;

            var layoutGroup = scrollContent.GetComponent<LayoutGroup>();
            if (layoutGroup != null) Destroy(layoutGroup);

            var sizeFitter = scrollContent.GetComponent<ContentSizeFitter>();
            if (sizeFitter != null) Destroy(sizeFitter);
        }

        _recyclableScrollRect?.Initialize(this);
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
        LoadInventoryFromFirebase(success =>
        {
            if (!success)
                Debug.Log("RecyclableInventoryManager: No saved inventory found, starting fresh.");

            GiveStarterPackIfNew();
        });
    }

    /// <summary>
    /// Schedules a deferred ReloadData to avoid Canvas rebuild conflicts.
    /// Multiple calls per frame are batched into a single reload.
    /// </summary>
    private void ScheduleReload()
    {
        if (_reloadPending) return;
        _reloadPending = true;

        if (_reloadCoroutine != null)
            StopCoroutine(_reloadCoroutine);

        _reloadCoroutine = StartCoroutine(DeferredReload());
    }

    private System.Collections.IEnumerator DeferredReload()
    {
        // Wait one frame to batch rapid inventory changes into a single reload.
        yield return null;
        _reloadPending = false;
        _reloadCoroutine = null;

        Debug.Log($"RecyclableInventoryManager: ReloadData with {_invenItems.Count} items");
        _recyclableScrollRect?.ReloadData();
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Loads inventory items from a list (called from LoadDataManager after Firebase load).
    /// </summary>
    public void SetLstItem(List<InvenItems> lst)
    {
        _invenItems = lst ?? new List<InvenItems>();
        ScheduleReload();
    }

    /// <summary>
    /// Forces a visible UI refresh without changing the inventory data.
    /// Useful when external state such as Quickbar assignment changes.
    /// </summary>
    public void RefreshUI()
    {
        ScheduleReload();
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

        // Phase 2.5E: Batched save timer
        if (hasPendingSave)
        {
            pendingSaveTimer -= Time.deltaTime;
            if (pendingSaveTimer <= 0f)
            {
                hasPendingSave = false;
                SaveInventoryToFirebase();
            }
        }
    }
    
    /// <summary>
    /// Toggles the inventory UI visibility via CanvasGroup (keeps GO active for coroutines).
    /// </summary>
    public void ToggleInventory()
    {
        bool isVisible = _inventoryCanvasGroup != null && _inventoryCanvasGroup.alpha > 0f;
        SetInventoryVisible(!isVisible);
    }

    /// <summary>
    /// Shows the inventory UI.
    /// </summary>
    public void ShowInventory() => SetInventoryVisible(true);

    /// <summary>
    /// Hides the inventory UI.
    /// </summary>
    public void HideInventory() => SetInventoryVisible(false);

    private void SetInventoryVisible(bool visible)
    {
        if (_inventoryCanvasGroup == null) return;
        _inventoryCanvasGroup.alpha = visible ? 1f : 0f;
        _inventoryCanvasGroup.interactable = visible;
        _inventoryCanvasGroup.blocksRaycasts = visible;

        if (visible)
        {
            ScheduleReload();
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
                        ScheduleReload();
                        ScheduleSave();
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
        ScheduleReload();
        ScheduleSave();
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
            ScheduleReload();
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

            ScheduleReload();
            ScheduleSave();
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
        ScheduleReload();
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

    // ==================== FIREBASE PERSISTENCE (Phase 2.5E) ====================

    /// <summary>
    /// Schedules a batched save to Firebase.
    /// Prevents rapid writes during fast harvesting or trading.
    /// </summary>
    private void ScheduleSave()
    {
        hasPendingSave = true;
        pendingSaveTimer = SAVE_BATCH_DELAY;
    }

    /// <summary>
    /// Immediately saves inventory to Firebase.
    /// Call before scene transitions or important actions.
    /// </summary>
    public void FlushSave()
    {
        if (hasPendingSave)
        {
            hasPendingSave = false;
            SaveInventoryToFirebase();
        }
    }

    /// <summary>
    /// Saves the current inventory to Firebase under Users/{userId}/inventory.
    /// Phase 2.5E Fix (BP3): Prevents inventory loss on crash.
    /// </summary>
    public void SaveInventoryToFirebase()
    {
        if (LoadDataManager.firebaseUser == null)
        {
            Debug.LogWarning("RecyclableInventoryManager: Cannot save — no Firebase user");
            return;
        }

        string userId = LoadDataManager.firebaseUser.UserId;
        string inventoryJson = JsonConvert.SerializeObject(_invenItems);

        FirebaseDatabaseManager.Instance?.WriteDatabase(
            FirebaseUserPaths.GetInventoryPath(userId),
            inventoryJson,
            (success, error) =>
            {
                if (success)
                {
                    Debug.Log($"RecyclableInventoryManager: Inventory saved ({_invenItems.Count} items)");
                }
                else
                {
                    Debug.LogError($"RecyclableInventoryManager: Failed to save inventory: {error}");
                }
            }
        );
    }

    /// <summary>
    /// Loads inventory from Firebase. Called during PlayScene initialization.
    /// </summary>
    public void LoadInventoryFromFirebase(System.Action<bool> onComplete = null)
    {
        if (LoadDataManager.firebaseUser == null)
        {
            Debug.LogWarning("RecyclableInventoryManager: Cannot load — no Firebase user");
            onComplete?.Invoke(false);
            return;
        }

        string userId = LoadDataManager.firebaseUser.UserId;

        FirebaseDatabaseManager.Instance?.ReadDatabase(
            FirebaseUserPaths.GetInventoryPath(userId),
            (data) =>
            {
                if (!string.IsNullOrEmpty(data))
                {
                    try
                    {
                        var loadedItems = JsonConvert.DeserializeObject<List<InvenItems>>(data);
                        if (loadedItems != null)
                        {
                            _invenItems = loadedItems;
                            ScheduleReload();
                            Debug.Log($"RecyclableInventoryManager: Loaded {_invenItems.Count} items from Firebase");
                        }
                        onComplete?.Invoke(true);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"RecyclableInventoryManager: Failed to parse inventory: {ex.Message}");
                        onComplete?.Invoke(false);
                    }
                }
                else
                {
                    Debug.Log("RecyclableInventoryManager: No saved inventory found");
                    onComplete?.Invoke(true);
                }
            }
        );
    }

    // ==================== STARTER PACK (SK-1) ====================

    /// <summary>
    /// Gives starter seeds to new players on first login.
    /// Checks Users/{userId}/starterPackGiven flag — only runs once per account.
    /// </summary>
    private void GiveStarterPackIfNew()
    {
        if (LoadDataManager.firebaseUser == null) return;

        string userId = LoadDataManager.firebaseUser.UserId;
        string flagPath = FirebaseUserPaths.GetStarterPackFlagPath(userId);

        FirebaseDatabaseManager.Instance?.ReadDatabase(flagPath, (data) =>
        {
            if (!string.IsNullOrEmpty(data)) return; // Already received starter pack

            // Give 5× Tomato seeds + 3× Wheat seeds
            AddInventoryItem(new InvenItems(
                itemId: "seed_tomato",
                name: "Hạt Cà Chua",
                description: "Hạt giống cà chua tươi ngon.",
                quantity: 5,
                itemType: "Seed",
                iconName: ""
            ));
            AddInventoryItem(new InvenItems(
                itemId: "seed_wheat",
                name: "Hạt Lúa Mì",
                description: "Hạt giống lúa mì vàng óng.",
                quantity: 3,
                itemType: "Seed",
                iconName: ""
            ));

            // Mark starter pack as given so it won't repeat
            FirebaseDatabaseManager.Instance?.WriteDatabase(flagPath, "true", null);

            FlushSave();
            NotificationManager.Instance?.ShowNotification(
                "Chào mừng! Bạn nhận được 5 Hạt Cà Chua và 3 Hạt Lúa Mì.", 3f);
            Debug.Log("RecyclableInventoryManager: Starter pack given to new player.");

            StartCoroutine(ShowTutorialHints());
        });
    }

    /// <summary>
    /// Shows sequential tutorial hints for first-time players (SK-2).
    /// Waits for each notification to finish before showing the next.
    /// </summary>
    private System.Collections.IEnumerator ShowTutorialHints()
    {
        yield return new WaitForSeconds(4f); // After welcome message fades
        NotificationManager.Instance?.ShowNotification("Nhấn [B] để mở túi đồ và chọn hạt giống.", 3.5f);
        yield return new WaitForSeconds(4.5f);
        NotificationManager.Instance?.ShowNotification("Chuột phải vào đất cỏ để cuốc đất.", 3.5f);
        yield return new WaitForSeconds(4.5f);
        NotificationManager.Instance?.ShowNotification("Chọn hạt (phím 1-9), rồi chuột phải vào đất đã cuốc để gieo.", 3.5f);
        yield return new WaitForSeconds(4.5f);
        NotificationManager.Instance?.ShowNotification("Chuột phải vào cây để tưới nước hoặc thu hoạch.", 3.5f);
        yield return new WaitForSeconds(4.5f);
        NotificationManager.Instance?.ShowNotification("Đến gặp người bán hàng và nhấn [E] để mua thêm hạt giống!", 4f);
    }

    void OnDestroy()
    {
        FlushSave();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            FlushSave();
        }
    }

    private void OnApplicationQuit()
    {
        FlushSave();
    }
}
