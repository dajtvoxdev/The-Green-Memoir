using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime manager for the plant growth state machine.
/// Tracks all active crops, updates growth stages based on elapsed time,
/// handles watering, harvesting, and syncs with Firebase via TileMapManager.
///
/// Phase 2 Feature (#4): Plant Growth State Machine
/// Phase 2 Feature (#25): Growth Persistence
/// </summary>
public class CropGrowthManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for global access.
    /// </summary>
    public static CropGrowthManager Instance { get; private set; }

    [Header("Crop Registry")]
    [Tooltip("All available CropDefinition assets. Drag them here in the Inspector.")]
    public CropDefinition[] cropDefinitions;

    [Header("Update Settings")]
    [Tooltip("How often (seconds) to check crop growth progress.")]
    public float updateInterval = 1f;

    [Header("References")]
    [Tooltip("Reference to the TileMapManager for tile data and Firebase sync.")]
    public TileMapManager tileMapManager;

    /// <summary>
    /// O(1) lookup: cropId -> CropDefinition.
    /// Built once from the cropDefinitions array at Start().
    /// </summary>
    private Dictionary<string, CropDefinition> cropRegistry;

    /// <summary>
    /// Set of tile coordinates that currently have active crops.
    /// Avoids scanning the entire tile cache every update cycle.
    /// </summary>
    private HashSet<(int x, int y)> activeCropTiles;

    /// <summary>
    /// Event fired when a crop changes growth stage.
    /// Parameters: x, y, oldStage, newStage, cropDefinition.
    /// </summary>
    public event Action<int, int, GrowthStage, GrowthStage, CropDefinition> OnCropStageChanged;

    /// <summary>
    /// Event fired when a crop becomes harvestable.
    /// Parameters: x, y, cropDefinition.
    /// </summary>
    public event Action<int, int, CropDefinition> OnCropHarvestable;

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
        BuildCropRegistry();
        ScanForActiveCrops();
        StartCoroutine(GrowthUpdateLoop());
    }

    /// <summary>
    /// Builds the cropId -> CropDefinition dictionary from the Inspector array.
    /// </summary>
    private void BuildCropRegistry()
    {
        cropRegistry = new Dictionary<string, CropDefinition>();

        if (cropDefinitions == null || cropDefinitions.Length == 0)
        {
            Debug.LogWarning("CropGrowthManager: No CropDefinitions assigned!");
            return;
        }

        foreach (var crop in cropDefinitions)
        {
            if (crop == null) continue;

            if (string.IsNullOrEmpty(crop.cropId))
            {
                Debug.LogWarning($"CropGrowthManager: CropDefinition '{crop.name}' has no cropId, skipping.");
                continue;
            }

            if (cropRegistry.ContainsKey(crop.cropId))
            {
                Debug.LogWarning($"CropGrowthManager: Duplicate cropId '{crop.cropId}', skipping '{crop.name}'.");
                continue;
            }

            cropRegistry[crop.cropId] = crop;
        }

        Debug.Log($"CropGrowthManager: Registered {cropRegistry.Count} crop definitions.");
    }

    /// <summary>
    /// Scans all loaded tiles to find tiles with active crops.
    /// Called once at startup to populate activeCropTiles.
    /// </summary>
    private void ScanForActiveCrops()
    {
        activeCropTiles = new HashSet<(int, int)>();

        if (LoadDataManager.userInGame?.MapInGame?.lstTilemapDetail == null)
        {
            Debug.LogWarning("CropGrowthManager: No map data to scan for crops.");
            return;
        }

        foreach (var tile in LoadDataManager.userInGame.MapInGame.lstTilemapDetail)
        {
            if (tile.HasCrop)
            {
                activeCropTiles.Add((tile.x, tile.y));
            }
        }

        Debug.Log($"CropGrowthManager: Found {activeCropTiles.Count} active crops on load.");
    }

    /// <summary>
    /// Coroutine that periodically checks all active crops and advances their growth stage.
    /// Uses time-based calculation rather than tick-by-tick increments.
    /// </summary>
    private IEnumerator GrowthUpdateLoop()
    {
        var wait = new WaitForSeconds(updateInterval);

        while (true)
        {
            yield return wait;
            UpdateAllCrops();
        }
    }

    /// <summary>
    /// Iterates all active crop tiles and updates their growth stage
    /// based on elapsed time since planting.
    /// </summary>
    private void UpdateAllCrops()
    {
        if (activeCropTiles == null || activeCropTiles.Count == 0) return;

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Copy to list to allow modification during iteration
        var tilesToCheck = new List<(int x, int y)>(activeCropTiles);

        foreach (var pos in tilesToCheck)
        {
            TilemapDetail tile = tileMapManager.GetTileAt(pos.x, pos.y);
            if (tile == null || !tile.HasCrop)
            {
                activeCropTiles.Remove(pos);
                continue;
            }

            if (!cropRegistry.TryGetValue(tile.cropId, out CropDefinition cropDef))
            {
                Debug.LogWarning($"CropGrowthManager: Unknown cropId '{tile.cropId}' at ({pos.x},{pos.y})");
                continue;
            }

            // Calculate elapsed seconds since planting
            float elapsedSeconds = (nowMs - tile.plantedAt) / 1000f;

            // Check watering requirement
            if (cropDef.requiresWater && tile.lastWateredAt == 0)
            {
                // Crop not watered — growth is paused
                continue;
            }

            // Determine the new growth stage from elapsed time
            GrowthStage newStage = cropDef.CalculateStageFromElapsed(elapsedSeconds);
            GrowthStage oldStage = (GrowthStage)tile.growthStage;

            if (newStage != oldStage)
            {
                tile.growthStage = (int)newStage;

                // Fire stage change event
                OnCropStageChanged?.Invoke(pos.x, pos.y, oldStage, newStage, cropDef);

                // Save to Firebase
                _ = tileMapManager.UpdateCropDataAsync(pos.x, pos.y, tile.cropId, (int)newStage);

                if (newStage == GrowthStage.Harvestable)
                {
                    OnCropHarvestable?.Invoke(pos.x, pos.y, cropDef);
                }

                Debug.Log($"CropGrowthManager: Crop '{cropDef.cropName}' at ({pos.x},{pos.y}) grew from {oldStage} to {newStage}");
            }
        }
    }

    // ==================== PUBLIC API ====================

    /// <summary>
    /// Gets the CropDefinition by cropId.
    /// Returns null if not found.
    /// </summary>
    public CropDefinition GetCropDefinition(string cropId)
    {
        if (string.IsNullOrEmpty(cropId) || cropRegistry == null) return null;
        cropRegistry.TryGetValue(cropId, out CropDefinition def);
        return def;
    }

    /// <summary>
    /// Plants a crop on the specified tile.
    /// Sets initial state and begins tracking growth.
    /// </summary>
    /// <param name="x">Tile X coordinate</param>
    /// <param name="y">Tile Y coordinate</param>
    /// <param name="cropId">The crop to plant (must exist in registry)</param>
    /// <returns>True if planting succeeded, false otherwise</returns>
    public bool PlantCrop(int x, int y, string cropId)
    {
        if (string.IsNullOrEmpty(cropId))
        {
            Debug.LogWarning("CropGrowthManager: Cannot plant — cropId is null/empty.");
            return false;
        }

        if (!cropRegistry.ContainsKey(cropId))
        {
            Debug.LogWarning($"CropGrowthManager: Cannot plant — unknown cropId '{cropId}'.");
            return false;
        }

        TilemapDetail tile = tileMapManager.GetTileAt(x, y);
        if (tile == null)
        {
            Debug.LogWarning($"CropGrowthManager: Cannot plant — no tile at ({x},{y}).");
            return false;
        }

        if (tile.HasCrop)
        {
            Debug.LogWarning($"CropGrowthManager: Cannot plant — tile ({x},{y}) already has a crop.");
            return false;
        }

        // Only allow planting on tilled ground
        if (tile.tilemapState != TilemapState.Ground)
        {
            Debug.LogWarning($"CropGrowthManager: Cannot plant — tile ({x},{y}) is not tilled ground.");
            return false;
        }

        // Set crop data on the tile
        tile.cropId = cropId;
        tile.plantedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        tile.growthStage = (int)GrowthStage.Seed;
        tile.lastWateredAt = 0;

        // Track this tile
        activeCropTiles.Add((x, y));

        // Persist to Firebase
        _ = tileMapManager.UpdateCropDataAsync(x, y, cropId, 0);

        CropDefinition cropDef = cropRegistry[cropId];
        Debug.Log($"CropGrowthManager: Planted '{cropDef.cropName}' at ({x},{y})");

        return true;
    }

    /// <summary>
    /// Waters the crop at the specified tile.
    /// Resets the watering timer to allow growth to continue.
    /// </summary>
    /// <param name="x">Tile X coordinate</param>
    /// <param name="y">Tile Y coordinate</param>
    /// <returns>True if watering succeeded</returns>
    public bool WaterCrop(int x, int y)
    {
        TilemapDetail tile = tileMapManager.GetTileAt(x, y);
        if (tile == null || !tile.HasCrop)
        {
            Debug.LogWarning($"CropGrowthManager: Cannot water — no crop at ({x},{y}).");
            return false;
        }

        tile.lastWateredAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // If crop hasn't started growing yet (plantedAt == 0 means timing issue),
        // reset planting time to now so growth starts from watering
        if (tile.plantedAt == 0)
        {
            tile.plantedAt = tile.lastWateredAt;
        }

        Debug.Log($"CropGrowthManager: Watered crop at ({x},{y})");
        return true;
    }

    /// <summary>
    /// Harvests the crop at the specified tile.
    /// Returns the harvested items, or null if harvest failed.
    /// </summary>
    /// <param name="x">Tile X coordinate</param>
    /// <param name="y">Tile Y coordinate</param>
    /// <returns>Harvested InvenItems with quantity, or null if failed</returns>
    public InvenItems HarvestCrop(int x, int y)
    {
        TilemapDetail tile = tileMapManager.GetTileAt(x, y);
        if (tile == null || !tile.HasCrop)
        {
            Debug.LogWarning($"CropGrowthManager: Cannot harvest — no crop at ({x},{y}).");
            return null;
        }

        GrowthStage currentStage = (GrowthStage)tile.growthStage;
        if (currentStage != GrowthStage.Harvestable)
        {
            Debug.LogWarning($"CropGrowthManager: Cannot harvest — crop at ({x},{y}) is at stage {currentStage}, not Harvestable.");
            return null;
        }

        if (!cropRegistry.TryGetValue(tile.cropId, out CropDefinition cropDef))
        {
            Debug.LogWarning($"CropGrowthManager: Cannot harvest — unknown cropId '{tile.cropId}'.");
            return null;
        }

        // Roll yield quantity
        int yield = cropDef.RollYield();

        // Create the harvested inventory item
        InvenItems harvestedItem = new InvenItems(
            itemId: cropDef.harvestItemId,
            name: cropDef.cropName,
            description: $"Freshly harvested {cropDef.cropName}",
            quantity: yield,
            itemType: "Crop",
            iconName: cropDef.cropName.ToLower().Replace(" ", "_")
        );

        // Handle regrowable vs one-time crops
        if (cropDef.regrowable)
        {
            // Reset to regrow stage instead of clearing
            tile.growthStage = (int)cropDef.regrowToStage;
            tile.plantedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            tile.lastWateredAt = 0;

            _ = tileMapManager.UpdateCropDataAsync(x, y, tile.cropId, tile.growthStage);
            Debug.Log($"CropGrowthManager: Harvested regrowable '{cropDef.cropName}' at ({x},{y}), yield={yield}, reset to {cropDef.regrowToStage}");
        }
        else
        {
            // One-time crop: clear the tile
            tile.ClearCrop();
            activeCropTiles.Remove((x, y));

            // Reset tile state back to Ground
            tileMapManager.SetStateForTilemapDetail(x, y, TilemapState.Ground);

            _ = tileMapManager.UpdateCropDataAsync(x, y, null, 0);
            Debug.Log($"CropGrowthManager: Harvested '{cropDef.cropName}' at ({x},{y}), yield={yield}, tile cleared.");
        }

        return harvestedItem;
    }

    /// <summary>
    /// Gets the current growth stage of the crop at the specified tile.
    /// Returns null if no crop exists at that position.
    /// </summary>
    public GrowthStage? GetCropStage(int x, int y)
    {
        TilemapDetail tile = tileMapManager.GetTileAt(x, y);
        if (tile == null || !tile.HasCrop) return null;
        return (GrowthStage)tile.growthStage;
    }

    /// <summary>
    /// Gets the growth progress (0.0 to 1.0) of the crop at the specified tile.
    /// Returns -1 if no crop exists.
    /// </summary>
    public float GetCropProgress(int x, int y)
    {
        TilemapDetail tile = tileMapManager.GetTileAt(x, y);
        if (tile == null || !tile.HasCrop) return -1f;

        if (!cropRegistry.TryGetValue(tile.cropId, out CropDefinition cropDef))
            return -1f;

        float elapsedSeconds = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - tile.plantedAt) / 1000f;
        float totalTime = cropDef.TotalGrowTime;

        if (totalTime <= 0f) return 1f;
        return Mathf.Clamp01(elapsedSeconds / totalTime);
    }

    /// <summary>
    /// Gets the number of actively growing crops.
    /// </summary>
    public int ActiveCropCount => activeCropTiles?.Count ?? 0;
}
