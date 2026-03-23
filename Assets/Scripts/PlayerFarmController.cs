using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

/// <summary>
/// Context-sensitive farming controller.
/// Right-click on any tile to auto-detect and perform the appropriate action:
///   Grass         → Till (clear grass, make plantable ground)
///   Tilled ground → Plant the selected seed (keys 1-9)
///   Crop, unwatered → Water
///   Crop, harvestable → Harvest
///   Crop, growing   → Show progress notification
///
/// Seed selection: number keys 1-9 (handled by SeedQuickbarUI).
/// The first seed is auto-selected by default when seeds exist.
/// </summary>
public class PlayerFarmController : MonoBehaviour
{
    [Header("Tilemap References")]
    public Tilemap tm_Ground;
    public Tilemap tm_Path;
    public Tilemap tm_Grass;
    public Tilemap tm_Forest;

    [Header("TileBase References")]
    public TileBase tb_Ground;
    public TileBase tb_Grass;
    public TileBase tb_Forest;

    [Tooltip("Tilled soil tile placed when grass is tilled. Assign a farmsoil tile asset.")]
    public TileBase tb_TilledSoil;

    [Header("Manager References")]
    public TileMapManager tileMapManager;
    public CropGrowthManager cropGrowthManager;

    [Header("Interaction")]
    [Tooltip("Max distance (in tiles) the player can interact with.")]
    public float maxInteractRange = 2.5f;

    [Tooltip("Only allow digging on cells that exist on Tilemap_Path.")]
    public bool requirePathTileForDig = true;

    [Tooltip("Physics layers treated as blocking obstacles for digging. Leave empty to detect all non-tilemap colliders.")]
    public LayerMask digObstacleMask = Physics2D.DefaultRaycastLayers;

    private Animator animator;
    private RecyclableInventoryManager recyclableInventoryManager;
    private static readonly Collider2D[] digOverlapBuffer = new Collider2D[16];

    public static PlayerFarmController Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        recyclableInventoryManager = FindObjectOfType<RecyclableInventoryManager>();

        if (tm_Path == null)
        {
            GameObject pathObject = GameObject.Find("Tilemap_Path");
            if (pathObject != null)
            {
                tm_Path = pathObject.GetComponent<Tilemap>();
            }
        }

        if (cropGrowthManager == null)
            cropGrowthManager = CropGrowthManager.Instance;
    }

    void Update()
    {
        // Right-click for context-sensitive farm action
        if (Input.GetMouseButtonDown(1))
        {
            // Skip when clicking on UI elements
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log($"[Farm] Right-click at world ({worldPos.x:F1},{worldPos.y:F1})");
            TryContextAction(worldPos);
        }
    }

    /// <summary>
    /// Attempts a context-sensitive farm action at the given world position.
    /// Uses TilemapDetail data as primary source of truth for tile state.
    /// Returns true if an action was performed.
    /// </summary>
    public bool TryContextAction(Vector3 worldPos)
    {
        Vector3Int cellPos = tm_Ground.WorldToCell(worldPos);

        // Range check — player must be close enough
        float dist = Vector2.Distance(
            transform.position,
            tm_Ground.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f));
        if (dist > maxInteractRange)
        {
            Debug.Log($"[Farm] Too far from ({cellPos.x},{cellPos.y}), dist={dist:F1}");
            return false;
        }

        // Get tile data from TileMapManager (primary source of truth)
        TilemapDetail tile = tileMapManager != null
            ? tileMapManager.GetTileAt(cellPos.x, cellPos.y)
            : null;

        // Fallback: also check visual grass tilemap for tiles not in the data model
        if (tile == null)
        {
            TileBase grassTile = tm_Grass != null ? tm_Grass.GetTile(cellPos) : null;
            bool hasOverlay = tm_Forest != null && tm_Forest.GetTile(cellPos) != null;
            if (grassTile != null && !hasOverlay)
            {
                if (!CanTillCell(cellPos, true))
                {
                    return false;
                }

                Debug.Log($"[Farm] No tile data but grass tile found at ({cellPos.x},{cellPos.y}), tilling");
                DoTill(cellPos);
                return true;
            }
            Debug.Log($"[Farm] No tile data at ({cellPos.x},{cellPos.y}), overlay={hasOverlay}");
            return false;
        }

        // 1. Tile state is Grass → Till
        // Data model is source of truth: Forest/Water tiles already have TilemapState.Forest
        if (tile.tilemapState == TilemapState.Grass)
        {
            if (!CanTillCell(cellPos, true))
            {
                return false;
            }

            DoTill(cellPos);
            return true;
        }

        // 2. Tile state is Forest → not farmable
        if (tile.tilemapState == TilemapState.Forest)
        {
            return false;
        }

        // 3. Tilled ground, no crop → Plant
        if (tile.tilemapState == TilemapState.Tilled && !tile.HasCrop)
        {
            DoPlant(cellPos);
            return true;
        }

        // 4. Has crop → context action based on growth stage
        if (tile.HasCrop)
        {
            GrowthStage stage = (GrowthStage)tile.growthStage;

            // Harvestable → Harvest (priority over watering)
            if (stage == GrowthStage.Harvestable)
            {
                DoHarvest(cellPos);
                return true;
            }

            // Needs water (lastWateredAt == 0 means never watered this cycle)
            if (tile.lastWateredAt == 0)
            {
                DoWater(cellPos);
                return true;
            }

            // Growing normally → show progress
            ShowGrowthProgress(tile, stage);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Tills the ground: clears grass and places tilled soil visual.
    /// </summary>
    private void DoTill(Vector3Int cellPos)
    {
        if (!CanTillCell(cellPos, true)) return;
        if (!StaminaCheck(StaminaManager.COST_TILL)) return;

        animator?.SetTrigger("Dig");

        // Set tile to Tilled state — TilemapDetailToUI handles both clearing
        // grass/forest overlays and placing the tilled soil visual.
        tileMapManager.SetStateForTilemapDetail(cellPos.x, cellPos.y, TilemapState.Tilled);
        AudioManager.Instance?.PlaySFX("till");
        TutorialManager.Instance?.CompleteStep(TutorialManager.TutorialStep.TillSoil);
        Debug.Log($"[Farm] Tilled ground at ({cellPos.x},{cellPos.y})");
    }

    private bool CanTillCell(Vector3Int cellPos, bool notifyPlayer)
    {
        if (requirePathTileForDig && tm_Path != null && !tm_Path.HasTile(cellPos))
        {
            if (notifyPlayer)
            {
                NotificationManager.Instance?.ShowNotification(
                    "Chỉ được đào đất ở ô đất trống.", 1.5f);
            }

            Debug.Log($"[Farm] Dig blocked at ({cellPos.x},{cellPos.y}) - no Tilemap_Path tile");
            return false;
        }

        if (HasBlockingObstacle(cellPos))
        {
            if (notifyPlayer)
            {
                NotificationManager.Instance?.ShowNotification(
                    "Ô này đang có vật cản, không thể cuốc đất.", 1.5f);
            }

            Debug.Log($"[Farm] Dig blocked at ({cellPos.x},{cellPos.y}) - obstacle detected");
            return false;
        }

        return true;
    }

    private bool HasBlockingObstacle(Vector3Int cellPos)
    {
        Vector3 worldCenter = tm_Ground.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0f);
        Vector2 overlapSize = new Vector2(0.8f, 0.8f);
        ContactFilter2D filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = digObstacleMask.value != 0,
            layerMask = digObstacleMask,
            useDepth = false,
            useNormalAngle = false,
        };

        int count = Physics2D.OverlapBox((Vector2)worldCenter, overlapSize, 0f, filter, digOverlapBuffer);
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = digOverlapBuffer[i];
            if (hit == null)
            {
                continue;
            }

            if (hit.transform == transform || hit.attachedRigidbody == GetComponent<Rigidbody2D>())
            {
                continue;
            }

            if (hit.GetComponent<TilemapCollider2D>() != null || hit.GetComponentInParent<Tilemap>() != null)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Plants the currently selected seed on tilled ground.
    /// Strips "seed_" prefix from inventory itemId to get the cropId
    /// that CropGrowthManager expects.
    /// </summary>
    private void DoPlant(Vector3Int cellPos)
    {
        // Get selected seed cropId from EquipmentManager
        string rawCropId = EquipmentManager.Instance?.CurrentSeedCropId;

        if (string.IsNullOrEmpty(rawCropId))
        {
            NotificationManager.Instance?.ShowNotification(
                "Chọn hạt giống trước! Nhấn 1-9 để chọn.", 2f);
            return;
        }

        // Fix cropId mismatch: inventory uses "seed_tomato", CropGrowthManager uses "tomato"
        string cropId = rawCropId.StartsWith("seed_") ? rawCropId.Substring(5) : rawCropId;

        if (cropGrowthManager == null)
        {
            Debug.LogError("PlayerFarm: CropGrowthManager not available!");
            return;
        }

        // Check inventory for seeds
        string seedItemId = rawCropId.StartsWith("seed_") ? rawCropId : "seed_" + rawCropId;
        if (!HasSeedInInventory(seedItemId))
        {
            NotificationManager.Instance?.ShowNotification(
                "Hết hạt giống! Mua thêm ở cửa hàng.", 2f);
            return;
        }

        if (!StaminaCheck(StaminaManager.COST_PLANT)) return;

        bool planted = cropGrowthManager.PlantCrop(cellPos.x, cellPos.y, cropId);

        if (planted)
        {
            ConsumeSeed(seedItemId);
            animator?.SetTrigger("Plant");
            tm_Forest.SetTile(cellPos, tb_Forest);
            AudioManager.Instance?.PlaySFX("plant");
            TutorialManager.Instance?.CompleteStep(TutorialManager.TutorialStep.PlantSeed);
        }
    }

    /// <summary>
    /// Waters the crop at the specified cell.
    /// </summary>
    private void DoWater(Vector3Int cellPos)
    {
        if (cropGrowthManager == null) return;
        if (!StaminaCheck(StaminaManager.COST_WATER)) return;

        animator?.SetTrigger("Water");
        cropGrowthManager.WaterCrop(cellPos.x, cellPos.y);
        AudioManager.Instance?.PlaySFX("water");
        TutorialManager.Instance?.CompleteStep(TutorialManager.TutorialStep.WaterCrop);
    }

    /// <summary>
    /// Harvests a crop and adds produce to inventory + earns gold.
    /// </summary>
    private void DoHarvest(Vector3Int cellPos)
    {
        if (cropGrowthManager == null) return;
        if (!StaminaCheck(StaminaManager.COST_HARVEST)) return;

        InvenItems harvestedItem = cropGrowthManager.HarvestCrop(cellPos.x, cellPos.y);
        if (harvestedItem == null) return;

        animator?.SetTrigger("Harvest");
        tm_Forest.SetTile(cellPos, null);

        // Check if crop is regrowable — keep visual if so
        TilemapDetail tile = tileMapManager.GetTileAt(cellPos.x, cellPos.y);
        if (tile != null && tile.HasCrop)
        {
            tm_Forest.SetTile(cellPos, tb_Forest);
        }
        else
        {
            tm_Grass.SetTile(cellPos, tb_Grass);
        }

        // Add to inventory
        if (recyclableInventoryManager != null)
        {
            recyclableInventoryManager.AddInventoryItem(harvestedItem);
        }

        // Earn gold
        CropDefinition cropDef = cropGrowthManager.GetCropDefinition(harvestedItem.itemId);
        int goldEarned = 0;
        if (cropDef != null && PlayerEconomyManager.Instance != null)
        {
            goldEarned = cropDef.sellPrice * harvestedItem.quantity;
            PlayerEconomyManager.Instance.EarnGold(goldEarned);
            PlayerEconomyManager.Instance.FlushSave();
        }

        AudioManager.Instance?.PlaySFX("harvest");
        TutorialManager.Instance?.CompleteStep(TutorialManager.TutorialStep.HarvestCrop);

        string goldMsg = goldEarned > 0 ? $" (+{goldEarned}G)" : "";
        NotificationManager.Instance?.ShowNotification(
            $"Thu hoạch {harvestedItem.quantity}x {harvestedItem.name}!{goldMsg}");
    }

    /// <summary>
    /// Shows a notification about the crop's current growth stage.
    /// </summary>
    private void ShowGrowthProgress(TilemapDetail tile, GrowthStage stage)
    {
        string stageName = stage switch
        {
            GrowthStage.Seed => "Hạt giống",
            GrowthStage.Sprout => "Nảy mầm",
            GrowthStage.Growing => "Đang lớn",
            GrowthStage.Mature => "Trưởng thành",
            GrowthStage.Harvestable => "Sẵn sàng thu hoạch!",
            _ => "Đang phát triển"
        };

        bool needsWater = tile.lastWateredAt == 0;
        string waterMsg = needsWater ? " (cần tưới nước!)" : "";
        NotificationManager.Instance?.ShowNotification(
            $"Cây trồng: {stageName}{waterMsg}", 1.5f);
    }

    /// <summary>
    /// Checks if player has at least 1 of the given seed in inventory.
    /// </summary>
    private bool HasSeedInInventory(string seedItemId)
    {
        if (recyclableInventoryManager == null) return false;

        var items = recyclableInventoryManager.GetInventoryItems();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemId == seedItemId && items[i].quantity > 0)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Consumes 1 seed from inventory.
    /// </summary>
    private void ConsumeSeed(string seedItemId)
    {
        if (recyclableInventoryManager == null) return;

        var items = recyclableInventoryManager.GetInventoryItems();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemId == seedItemId && items[i].quantity > 0)
            {
                recyclableInventoryManager.RemoveQuantityAt(i, 1);
                return;
            }
        }
    }

    /// <summary>
    /// Checks stamina. Returns false if insufficient.
    /// Passes through if StaminaManager is not present.
    /// </summary>
    private bool StaminaCheck(int cost)
    {
        return StaminaManager.Instance == null || StaminaManager.Instance.TrySpendStamina(cost);
    }
}
