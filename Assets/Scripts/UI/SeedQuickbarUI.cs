using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Seed quickbar — horizontal slot bar showing seeds from inventory.
/// Players press number keys (1-9) to quickly select a seed for planting.
/// When selected, equips a SeedBag tool via EquipmentManager.
///
/// Phase 3: Configurable quickbar — players assign seeds from inventory.
/// If no assignments exist, falls back to auto-fill with all inventory seeds.
/// Assignments persist via Firebase at Users/{userId}/quickbarSeeds.
///
/// Subscribes to RecyclableInventoryManager.OnInventoryChanged to auto-refresh.
/// </summary>
public class SeedQuickbarUI : MonoBehaviour
{
    public static SeedQuickbarUI Instance { get; private set; }

    public event System.Action OnAssignmentsChanged;

    [Header("References")]
    [Tooltip("Parent transform for seed slots. If null, uses this transform.")]
    public Transform slotContainer;

    [Header("Seed Icons")]
    [Tooltip("Known seed icon mappings. Populated by editor setup script.")]
    [SerializeField] private SeedIconEntry[] seedIcons;

    [Header("Layout")]
    [Tooltip("Maximum size of each seed slot in pixels.")]
    public Vector2 slotSize = new Vector2(80, 80);

    [Tooltip("Spacing between slots.")]
    public float slotSpacing = 6f;

    [Tooltip("Padding around the bar.")]
    public float padding = 8f;

    [Tooltip("Maximum total width for the seed bar (pixels).")]
    public float maxBarWidth = 900f;

    [Header("Key Bindings")]
    [Tooltip("First key code for seed selection (default: 1).")]
    public KeyCode firstKey = KeyCode.Alpha1;

    [Tooltip("Maximum number of seed slots.")]
    public int maxSlots = 9;

    private readonly List<SeedSlotData> slots = new List<SeedSlotData>();
    private int activeIndex = -1;
    private RecyclableInventoryManager inventoryManager;
    private CanvasGroup _canvasGroup;

    // Configurable quickbar: ordered list of assigned seed itemIds
    private List<string> _assignedSeeds = new List<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    [System.Serializable]
    public class SeedIconEntry
    {
        public string itemIdPrefix;
        public Sprite icon;
    }

    private class SeedSlotData
    {
        public GameObject slotGO;
        public Image backgroundImage;
        public Image iconImage;
        public TMP_Text keyText;
        public TMP_Text qtyText;
        public TMP_Text nameText;
        public InvenItems seedItem;
    }

    void Start()
    {
        if (slotContainer == null) slotContainer = transform;

        // CanvasGroup for hiding the bar when no seeds are present
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        SetBarVisible(false); // hidden until seeds are found

        EnsureLayoutGroup();

        inventoryManager = FindObjectOfType<RecyclableInventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged += RefreshSeeds;
        }

        // Subscribe to tool changes to sync highlight
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnToolChanged += OnToolChanged;
        }

        // Load quickbar assignments from Firebase, then build slots
        LoadQuickbarAssignments(() =>
        {
            Invoke(nameof(RefreshSeeds), 0.3f);
        });
    }

    void Update()
    {
        HandleSeedInput();
    }

    /// <summary>
    /// Handles number key input for quick seed switching (keys 1-9).
    /// </summary>
    private void HandleSeedInput()
    {
        for (int i = 0; i < slots.Count && i < maxSlots; i++)
        {
            if (Input.GetKeyDown(firstKey + i))
            {
                SelectSeed(i);
                break;
            }
        }
    }

    /// <summary>
    /// Selects a seed slot and equips it as a SeedBag tool.
    /// </summary>
    private void SelectSeed(int index)
    {
        if (index < 0 || index >= slots.Count) return;

        // Deselect previous
        if (activeIndex >= 0 && activeIndex < slots.Count)
        {
            SetSlotHighlight(activeIndex, false);
        }

        activeIndex = index;
        SetSlotHighlight(index, true);

        EquipSeedAsTool(slots[index].seedItem);
    }

    /// <summary>
    /// Equips a seed as a SeedBag tool via EquipmentManager.
    /// Reuses existing SeedBag if available, otherwise creates one at runtime.
    /// </summary>
    private void EquipSeedAsTool(InvenItems seedItem)
    {
        if (EquipmentManager.Instance == null || seedItem == null) return;

        string cropId = seedItem.itemId;

        // Try to find an existing SeedBag tool with matching cropId
        ToolDefinition matchingTool = null;
        if (EquipmentManager.Instance.starterTools != null)
        {
            foreach (var tool in EquipmentManager.Instance.starterTools)
            {
                if (tool != null && tool.toolType == ToolType.SeedBag && tool.cropId == cropId)
                {
                    matchingTool = tool;
                    break;
                }
            }
        }

        if (matchingTool != null)
        {
            EquipmentManager.Instance.EquipTool(matchingTool);
        }
        else
        {
            // Create a runtime SeedBag tool
            var runtimeTool = ScriptableObject.CreateInstance<ToolDefinition>();
            runtimeTool.toolId = $"seedbag_{seedItem.itemId}";
            runtimeTool.toolName = seedItem.name;
            runtimeTool.description = seedItem.description;
            runtimeTool.toolType = ToolType.SeedBag;
            runtimeTool.cropId = cropId;
            runtimeTool.tier = 1;

            // Try to find icon from seed icon mappings
            Sprite icon = FindSeedIcon(seedItem);
            if (icon != null) runtimeTool.icon = icon;

            EquipmentManager.Instance.EquipTool(runtimeTool);
        }
    }

    /// <summary>
    /// Refreshes seed slots from current inventory contents.
    /// If quickbar assignments exist, shows only assigned seeds in order.
    /// Otherwise, auto-fills with all inventory seeds (legacy behavior).
    /// </summary>
    public void RefreshSeeds()
    {
        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<RecyclableInventoryManager>();
            if (inventoryManager == null) return;

            // Late subscription — Start() couldn't find the manager earlier
            inventoryManager.OnInventoryChanged += RefreshSeeds;
        }

        var allItems = inventoryManager.GetInventoryItems();

        // Build lookup of available seeds in inventory
        var seedLookup = new Dictionary<string, InvenItems>();
        foreach (var item in allItems)
        {
            if (item.itemId != null && item.itemId.StartsWith("seed_"))
            {
                seedLookup[item.itemId] = item;
            }
        }

        var seeds = new List<InvenItems>();

        if (_assignedSeeds.Count > 0)
        {
            // Configurable mode: show only assigned seeds that still exist in inventory
            foreach (var seedId in _assignedSeeds)
            {
                if (seedLookup.TryGetValue(seedId, out InvenItems seed))
                {
                    seeds.Add(seed);
                }
            }
        }
        else
        {
            // Auto-fill mode: show all seeds from inventory (legacy behavior)
            seeds.AddRange(seedLookup.Values);
        }

        BuildSlots(seeds);
    }

    // ==================== QUICKBAR ASSIGNMENT API ====================

    /// <summary>
    /// Checks if a seed is currently assigned to the quickbar.
    /// </summary>
    public bool IsSeedInQuickbar(string itemId)
    {
        return _assignedSeeds.Contains(itemId);
    }

    public int GetAssignedSlotIndex(string itemId)
    {
        return _assignedSeeds.IndexOf(itemId);
    }

    /// <summary>
    /// Assigns a seed to the next available quickbar slot.
    /// Returns false if quickbar is full or seed is already assigned.
    /// </summary>
    public bool AssignToQuickbar(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return false;
        if (_assignedSeeds.Contains(itemId)) return false;
        if (_assignedSeeds.Count >= maxSlots) return false;

        _assignedSeeds.Add(itemId);
        SaveQuickbarAssignments();
        OnAssignmentsChanged?.Invoke();
        RefreshSeeds();
        return true;
    }

    /// <summary>
    /// Removes a seed from the quickbar assignments.
    /// </summary>
    public void RemoveFromQuickbar(string itemId)
    {
        if (_assignedSeeds.Remove(itemId))
        {
            SaveQuickbarAssignments();
            OnAssignmentsChanged?.Invoke();
            RefreshSeeds();
        }
    }

    /// <summary>
    /// Clears all quickbar assignments (reverts to auto-fill mode).
    /// </summary>
    public void ClearQuickbarAssignments()
    {
        _assignedSeeds.Clear();
        SaveQuickbarAssignments();
        OnAssignmentsChanged?.Invoke();
        RefreshSeeds();
    }

    // ==================== FIREBASE PERSISTENCE ====================

    private void LoadQuickbarAssignments(System.Action onComplete = null)
    {
        if (LoadDataManager.firebaseUser == null)
        {

            onComplete?.Invoke();
            return;
        }

        string userId = LoadDataManager.firebaseUser.UserId;
        string path = $"Users/{userId}/quickbarSeeds";

        FirebaseDatabaseManager.Instance?.ReadDatabase(path, (data) =>
        {
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    var loaded = JsonConvert.DeserializeObject<List<string>>(data);
                    if (loaded != null)
                    {
                        _assignedSeeds = loaded;
                        Debug.Log($"SeedQuickbarUI: Loaded {_assignedSeeds.Count} quickbar assignments");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"SeedQuickbarUI: Failed to parse quickbar assignments: {ex.Message}");
                }
            }


            onComplete?.Invoke();
        });
    }

    private void SaveQuickbarAssignments()
    {
        if (LoadDataManager.firebaseUser == null) return;

        string userId = LoadDataManager.firebaseUser.UserId;
        string path = $"Users/{userId}/quickbarSeeds";
        string json = JsonConvert.SerializeObject(_assignedSeeds);

        FirebaseDatabaseManager.Instance?.WriteDatabase(path, json, (success, error) =>
        {
            if (!success)
                Debug.LogError($"SeedQuickbarUI: Failed to save quickbar: {error}");
        });
    }

    /// <summary>
    /// Rebuilds seed slot UI elements.
    /// </summary>
    private void BuildSlots(List<InvenItems> seeds)
    {
        // Clear existing
        foreach (var slot in slots)
        {
            if (slot.slotGO != null) Destroy(slot.slotGO);
        }
        slots.Clear();
        activeIndex = -1;

        // Hide the bar entirely when no seeds are present
        if (seeds.Count == 0)
        {
            SetBarVisible(false);
            return;
        }

        // Create slots for each seed (up to maxSlots)
        int count = Mathf.Min(seeds.Count, maxSlots);

        // Calculate adaptive slot size to fit all seeds within maxBarWidth
        Vector2 adaptedSize = CalculateAdaptiveSlotSize(count);

        for (int i = 0; i < count; i++)
        {
            var slotData = CreateSlot(i, seeds[i], adaptedSize);
            slots.Add(slotData);
        }

        SetBarVisible(true);

        // Auto-highlight if current tool matches a seed, or auto-select first
        SyncHighlightWithCurrentTool();

        // Auto-select first seed if nothing is currently selected
        if (activeIndex < 0 && slots.Count > 0)
        {
            SelectSeed(0);
        }
    }

    /// <summary>
    /// Calculates adaptive slot size so all seeds fit within maxBarWidth.
    /// </summary>
    private Vector2 CalculateAdaptiveSlotSize(int slotCount)
    {
        float totalSpacing = slotSpacing * (slotCount - 1);
        float totalPadding = padding * 2;
        float available = maxBarWidth - totalSpacing - totalPadding;
        float fitWidth = available / slotCount;

        // Clamp between a minimum size and the configured slotSize
        float finalWidth = Mathf.Clamp(fitWidth, 48f, slotSize.x);
        float finalHeight = finalWidth * (slotSize.y / slotSize.x);
        return new Vector2(finalWidth, finalHeight);
    }

    /// <summary>
    /// Creates a single seed slot with icon, key label, and quantity.
    /// </summary>
    private SeedSlotData CreateSlot(int index, InvenItems seed, Vector2 size)
    {
        var data = new SeedSlotData { seedItem = seed };

        // Root slot — warm wood background with outline
        var root = new GameObject($"SeedSlot_{index}", typeof(RectTransform), typeof(Image), typeof(UnityEngine.UI.Outline));
        root.transform.SetParent(slotContainer, false);
        data.slotGO = root;

        var rootRT = root.GetComponent<RectTransform>();
        rootRT.sizeDelta = size;

        data.backgroundImage = root.GetComponent<Image>();
        data.backgroundImage.color = new Color(0.55f, 0.42f, 0.28f, 0.92f);

        var outline = root.GetComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0.2f, 0.15f, 0.08f, 0.6f);
        outline.effectDistance = new Vector2(2f, -2f);

        // === ICON — centered, smaller within the slot ===
        var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGO.transform.SetParent(root.transform, false);
        var iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.2f, 0.2f);
        iconRT.anchorMax = new Vector2(0.8f, 0.8f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;

        data.iconImage = iconGO.GetComponent<Image>();
        data.iconImage.preserveAspect = true;

        Sprite icon = FindSeedIcon(seed);
        if (icon != null)
        {
            data.iconImage.sprite = icon;
        }
        else
        {
            data.iconImage.color = new Color(0.45f, 0.7f, 0.35f, 0.8f);
        }

        // === KEY NUMBER — tiny badge, top-left corner ===
        var keyGO = new GameObject("KeyLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        keyGO.transform.SetParent(root.transform, false);
        var keyRT = keyGO.GetComponent<RectTransform>();
        keyRT.anchorMin = new Vector2(0f, 0.82f);
        keyRT.anchorMax = new Vector2(0.22f, 1f);
        keyRT.offsetMin = Vector2.zero;
        keyRT.offsetMax = Vector2.zero;

        // Scale font sizes proportionally to slot size
        float fontScale = size.x / 80f;

        data.keyText = keyGO.GetComponent<TextMeshProUGUI>();
        data.keyText.text = (1 + index).ToString();
        data.keyText.fontSize = Mathf.Max(10f, 16f * fontScale);
        data.keyText.fontStyle = FontStyles.Bold;
        data.keyText.alignment = TextAlignmentOptions.TopLeft;
        data.keyText.color = new Color(1f, 1f, 1f, 0.85f);
        data.keyText.margin = new Vector4(2, 1, 0, 0);

        // === QUANTITY — small badge, bottom-right corner ===
        var qtyGO = new GameObject("QtyLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        qtyGO.transform.SetParent(root.transform, false);
        var qtyRT = qtyGO.GetComponent<RectTransform>();
        qtyRT.anchorMin = new Vector2(0.7f, 0f);
        qtyRT.anchorMax = new Vector2(1f, 0.22f);
        qtyRT.offsetMin = Vector2.zero;
        qtyRT.offsetMax = Vector2.zero;

        data.qtyText = qtyGO.GetComponent<TextMeshProUGUI>();
        data.qtyText.text = seed.quantity > 1 ? seed.quantity.ToString() : "";
        data.qtyText.fontSize = Mathf.Max(10f, 18f * fontScale);
        data.qtyText.fontStyle = FontStyles.Bold;
        data.qtyText.alignment = TextAlignmentOptions.BottomRight;
        data.qtyText.color = new Color(1f, 0.92f, 0.3f, 1f);
        data.qtyText.margin = new Vector4(0, 0, 2, 1);

        // === SEED NAME — below the slot ===
        var nameGO = new GameObject("NameLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameGO.transform.SetParent(root.transform, false);
        var nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(-0.1f, -0.35f);
        nameRT.anchorMax = new Vector2(1.1f, 0f);
        nameRT.offsetMin = Vector2.zero;
        nameRT.offsetMax = Vector2.zero;

        data.nameText = nameGO.GetComponent<TextMeshProUGUI>();
        data.nameText.text = GetShortName(seed.name);
        data.nameText.fontSize = Mathf.Max(9f, 14f * fontScale);
        data.nameText.alignment = TextAlignmentOptions.Top;
        data.nameText.color = new Color(1f, 0.95f, 0.8f, 0.95f);
        data.nameText.overflowMode = TextOverflowModes.Ellipsis;

        return data;
    }

    /// <summary>
    /// Finds the icon sprite for a seed item.
    /// Tries serialized seedIcons first, then falls back to ShopManager catalog lookup.
    /// </summary>
    private Sprite FindSeedIcon(InvenItems seed)
    {
        if (seed == null) return null;

        // Pass 1: serialized seedIcons array
        if (seedIcons != null)
        {
            foreach (var entry in seedIcons)
            {
                if (entry.icon != null && seed.itemId.Contains(entry.itemIdPrefix))
                {
                    return entry.icon;
                }
            }
        }

        // Pass 2: lookup from ShopManager catalog (ItemDefinition.icon)
        if (ShopManager.Instance?.CurrentCatalog?.entries != null)
        {
            foreach (var entry in ShopManager.Instance.CurrentCatalog.entries)
            {
                if (entry.item != null && entry.item.itemId == seed.itemId && entry.item.icon != null)
                {
                    return entry.item.icon;
                }
            }
        }

        // Pass 3: lookup from all loaded ItemDefinitions via Resources
        var allItems = Resources.FindObjectsOfTypeAll<ItemDefinition>();
        foreach (var item in allItems)
        {
            if (item.itemId == seed.itemId && item.icon != null)
            {
                return item.icon;
            }
        }

        return null;
    }

    /// <summary>
    /// Shortens a seed name for display under the slot.
    /// </summary>
    private string GetShortName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return "";
        // Remove common prefixes like "Hạt "
        if (fullName.StartsWith("Hạt ")) return fullName.Substring(4);
        return fullName;
    }

    /// <summary>
    /// Sets the visual highlight state of a slot.
    /// </summary>
    private void SetSlotHighlight(int index, bool highlighted)
    {
        if (index < 0 || index >= slots.Count) return;

        var bg = slots[index].backgroundImage;
        if (bg == null) return;

        bg.color = highlighted
            ? new Color(0.85f, 0.65f, 0.2f, 1f) // warm golden highlight
            : new Color(0.55f, 0.42f, 0.28f, 0.92f); // wood brown default
    }

    /// <summary>
    /// Syncs the active seed highlight with the currently equipped tool.
    /// </summary>
    private void SyncHighlightWithCurrentTool()
    {
        if (EquipmentManager.Instance == null) return;

        var currentTool = EquipmentManager.Instance.CurrentTool;
        if (currentTool == null || currentTool.toolType != ToolType.SeedBag)
        {
            // No seed tool equipped — deselect all
            if (activeIndex >= 0 && activeIndex < slots.Count)
            {
                SetSlotHighlight(activeIndex, false);
            }
            activeIndex = -1;
            return;
        }

        string cropId = currentTool.cropId;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].seedItem.itemId == cropId)
            {
                if (activeIndex >= 0 && activeIndex < slots.Count)
                    SetSlotHighlight(activeIndex, false);
                activeIndex = i;
                SetSlotHighlight(i, true);
                return;
            }
        }
    }

    /// <summary>
    /// Handles tool change from EquipmentManager to sync highlight.
    /// </summary>
    private void OnToolChanged(ToolDefinition tool)
    {
        SyncHighlightWithCurrentTool();
    }

    /// <summary>
    /// Ensures the slot container has a HorizontalLayoutGroup.
    /// </summary>
    private void EnsureLayoutGroup()
    {
        if (slotContainer == null) return;

        var hlg = slotContainer.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null)
        {
            hlg = slotContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        hlg.spacing = slotSpacing;
        hlg.padding = new RectOffset(
            Mathf.RoundToInt(padding),
            Mathf.RoundToInt(padding),
            Mathf.RoundToInt(padding),
            Mathf.RoundToInt(padding)
        );
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        var csf = slotContainer.GetComponent<ContentSizeFitter>();
        if (csf == null)
        {
            csf = slotContainer.gameObject.AddComponent<ContentSizeFitter>();
        }
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    /// <summary>
    /// Shows or hides the entire seed quickbar via CanvasGroup.
    /// </summary>
    private void SetBarVisible(bool visible)
    {
        if (_canvasGroup == null) return;
        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.blocksRaycasts = visible;
        _canvasGroup.interactable = visible;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged -= RefreshSeeds;
        }
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnToolChanged -= OnToolChanged;
        }
    }
}
