using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Handles player farm actions: till, plant, water, harvest.
/// Phase 2 refactor: Delegates crop logic to CropGrowthManager.
/// Phase 2 update: Integrates with EquipmentManager for tool-based actions.
///
/// Key bindings:
///   E = Use equipped tool (context-sensitive based on EquipmentManager)
///   C = Till (clear grass to make plantable ground)
///   V = Plant seed on tilled ground
///   F = Water the crop
///   M = Harvest a harvestable crop
///   1-6 = Switch tools (via EquipmentManager)
/// </summary>
public class PlayerFarmController : MonoBehaviour
{
    [Header("Tilemap References")]
    public Tilemap tm_Ground;
    public Tilemap tm_Grass;
    public Tilemap tm_Forest;

    [Header("TileBase References")]
    public TileBase tb_Ground;
    public TileBase tb_Grass;
    public TileBase tb_Forest;

    [Header("Manager References")]
    public TileMapManager tileMapManager;
    public CropGrowthManager cropGrowthManager;

    [Header("Planting Settings")]
    [Tooltip("Default cropId used when no tool system or seed is selected. Fallback for testing.")]
    public string defaultCropId = "crop_tomato";

    private Animator animator;
    private RecyclableInventoryManager recyclableInventoryManager;

    private void Start()
    {
        animator = GetComponent<Animator>();
        recyclableInventoryManager = GameObject.Find("InventoryManager")?.GetComponent<RecyclableInventoryManager>();

        if (cropGrowthManager == null)
        {
            cropGrowthManager = CropGrowthManager.Instance;
        }
    }

    void Update()
    {
        HandleFarmAction();
    }

    /// <summary>
    /// Routes input keys to the appropriate farm action.
    /// E key uses the equipped tool; legacy keys still work as direct shortcuts.
    /// </summary>
    public void HandleFarmAction()
    {
        // Tool-based action (E key)
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleToolAction();
        }

        // Legacy direct keys (still functional for quick testing)
        if (Input.GetKeyDown(KeyCode.C))
        {
            HandleTill();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            HandlePlant();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            HandleWater();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            HandleHarvest();
        }
    }

    /// <summary>
    /// Uses the currently equipped tool at the player's position.
    /// Routes to the appropriate action based on EquipmentManager.CurrentToolType.
    /// </summary>
    private void HandleToolAction()
    {
        if (EquipmentManager.Instance == null || EquipmentManager.Instance.CurrentTool == null)
        {
            NotificationManager.Instance?.ShowNotification("No tool equipped! Press 1-4 to select.", 1.5f);
            return;
        }

        switch (EquipmentManager.Instance.CurrentToolType)
        {
            case ToolType.Hoe:
                HandleTill();
                break;
            case ToolType.SeedBag:
                HandlePlantWithSeed();
                break;
            case ToolType.WateringCan:
                HandleWater();
                break;
            case ToolType.Sickle:
                HandleHarvest();
                break;
            default:
                NotificationManager.Instance?.ShowNotification("This tool can't be used here.", 1.5f);
                break;
        }
    }

    /// <summary>
    /// Gets the tile cell position under the player.
    /// </summary>
    private Vector3Int GetPlayerCellPos()
    {
        return tm_Ground.WorldToCell(transform.position);
    }

    /// <summary>
    /// Tills the ground: clears grass to create plantable ground.
    /// </summary>
    private void HandleTill()
    {
        Vector3Int cellPos = GetPlayerCellPos();
        TileBase currentGrassTile = tm_Grass.GetTile(cellPos);

        if (currentGrassTile == tb_Grass)
        {
            animator?.SetTrigger("Dig");
            tm_Grass.SetTile(cellPos, null);
            tileMapManager.SetStateForTilemapDetail(cellPos.x, cellPos.y, TilemapState.Ground);
            AudioManager.Instance?.PlaySFX("till");
            Debug.Log($"PlayerFarm: Tilled ground at ({cellPos.x},{cellPos.y})");
        }
    }

    /// <summary>
    /// Plants a seed on tilled ground using CropGrowthManager.
    /// Uses defaultCropId as fallback when no seed is selected.
    /// </summary>
    private void HandlePlant()
    {
        // Try to get cropId from equipped seed, fall back to default
        string cropId = EquipmentManager.Instance?.CurrentSeedCropId ?? defaultCropId;
        PlantCropAtPlayer(cropId);
    }

    /// <summary>
    /// Plants using the seed from the currently equipped SeedBag tool.
    /// </summary>
    private void HandlePlantWithSeed()
    {
        string cropId = EquipmentManager.Instance?.CurrentSeedCropId;

        if (string.IsNullOrEmpty(cropId))
        {
            NotificationManager.Instance?.ShowNotification("No seed selected!", 1.5f);
            return;
        }

        PlantCropAtPlayer(cropId);
    }

    /// <summary>
    /// Core planting logic — shared by HandlePlant and HandlePlantWithSeed.
    /// </summary>
    private void PlantCropAtPlayer(string cropId)
    {
        Vector3Int cellPos = GetPlayerCellPos();

        if (cropGrowthManager == null)
        {
            Debug.LogError("PlayerFarm: CropGrowthManager not available!");
            return;
        }

        bool planted = cropGrowthManager.PlantCrop(cellPos.x, cellPos.y, cropId);

        if (planted)
        {
            animator?.SetTrigger("Plant");
            tm_Forest.SetTile(cellPos, tb_Forest);
            AudioManager.Instance?.PlaySFX("plant");
        }
    }

    /// <summary>
    /// Waters the crop at the player's current position.
    /// </summary>
    private void HandleWater()
    {
        Vector3Int cellPos = GetPlayerCellPos();

        if (cropGrowthManager == null)
        {
            Debug.LogError("PlayerFarm: CropGrowthManager not available!");
            return;
        }

        animator?.SetTrigger("Water");
        cropGrowthManager.WaterCrop(cellPos.x, cellPos.y);
        AudioManager.Instance?.PlaySFX("water");
    }

    /// <summary>
    /// Harvests a crop and adds items to inventory.
    /// </summary>
    private void HandleHarvest()
    {
        Vector3Int cellPos = GetPlayerCellPos();

        if (cropGrowthManager == null)
        {
            Debug.LogError("PlayerFarm: CropGrowthManager not available!");
            return;
        }

        InvenItems harvestedItem = cropGrowthManager.HarvestCrop(cellPos.x, cellPos.y);

        if (harvestedItem != null)
        {
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
                Debug.Log($"PlayerFarm: Harvested {harvestedItem.quantity}x '{harvestedItem.name}'");
            }

            // Earn Gold from selling crop
            CropDefinition cropDef = cropGrowthManager.GetCropDefinition(harvestedItem.itemId);
            int goldEarned = 0;
            if (cropDef != null && PlayerEconomyManager.Instance != null)
            {
                goldEarned = cropDef.sellPrice * harvestedItem.quantity;
                PlayerEconomyManager.Instance.EarnGold(goldEarned);
            }

            // Play harvest SFX
            AudioManager.Instance?.PlaySFX("harvest");

            // Show notification
            string goldMsg = goldEarned > 0 ? $" (+{goldEarned}G)" : "";
            NotificationManager.Instance?.ShowNotification($"Harvested {harvestedItem.quantity}x {harvestedItem.name}!{goldMsg}");
        }
    }
}
