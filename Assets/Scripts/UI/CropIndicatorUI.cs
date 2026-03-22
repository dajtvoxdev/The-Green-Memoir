using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Displays visual indicators above crops to show their status:
/// - Water droplet icon when crop needs watering
/// - Exclamation mark (!) when crop is ready to harvest
/// - Growth progress bar showing current stage
///
/// Phase 2.5D Fix (BP5, BP6, BP7): Provides visual feedback for crop states.
///
/// Setup:
///   Attach to a GameObject in PlayScene. Assign tileMapManager reference.
///   Creates indicators as child GameObjects with SpriteRenderers.
/// </summary>
public class CropIndicatorUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to TileMapManager to read crop data.")]
    public TileMapManager tileMapManager;

    [Tooltip("Reference to CropGrowthManager for growth events.")]
    public CropGrowthManager cropGrowthManager;

    [Tooltip("Reference to the Ground tilemap for cell-to-world conversion.")]
    public Tilemap groundTilemap;

    [Header("Indicator Settings")]
    [Tooltip("Color for 'needs water' indicator.")]
    public Color waterColor = new Color(0.3f, 0.6f, 1.0f, 0.9f);

    [Tooltip("Color for 'ready to harvest' indicator.")]
    public Color harvestColor = new Color(1.0f, 0.8f, 0.0f, 0.9f);

    [Tooltip("Color for growth progress bar background.")]
    public Color progressBgColor = new Color(0.2f, 0.2f, 0.2f, 0.6f);

    [Tooltip("Color for growth progress bar fill.")]
    public Color progressFillColor = new Color(0.2f, 0.8f, 0.2f, 0.9f);

    [Tooltip("How often to refresh indicators (seconds).")]
    public float refreshInterval = 1.0f;

    [Header("Custom Icons")]
    [Tooltip("Sprite for 'needs water' indicator. Falls back to circle if null.")]
    public Sprite waterIconSprite;

    [Tooltip("Sprite for 'ready to harvest' indicator. Falls back to circle if null.")]
    public Sprite harvestIconSprite;

    [Header("Positioning")]
    [Tooltip("Vertical offset above tile center for indicators.")]
    public float yOffset = 0.8f;

    // Active indicator GameObjects keyed by (x, y) tile coords
    private Dictionary<(int, int), GameObject> activeIndicators = new Dictionary<(int, int), GameObject>();

    private float refreshTimer;

    void Start()
    {
        if (cropGrowthManager == null)
        {
            cropGrowthManager = CropGrowthManager.Instance;
        }

        // Subscribe to crop events for immediate feedback
        if (cropGrowthManager != null)
        {
            cropGrowthManager.OnCropStageChanged += OnCropStageChanged;
            cropGrowthManager.OnCropHarvestable += OnCropHarvestable;
        }
    }

    void Update()
    {
        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f)
        {
            refreshTimer = refreshInterval;
            RefreshAllIndicators();
        }
    }

    /// <summary>
    /// Refreshes all crop indicators by scanning active crop tiles.
    /// </summary>
    private void RefreshAllIndicators()
    {
        if (tileMapManager == null || groundTilemap == null) return;

        // Get all tiles with crops from the tile cache
        var allTiles = tileMapManager.GetAllTiles();
        if (allTiles == null) return;

        // Track which tiles still have crops
        var activeTiles = new HashSet<(int, int)>();

        foreach (var kvp in allTiles)
        {
            var tile = kvp.Value;
            if (tile == null || !tile.HasCrop) continue;

            int x = tile.x;
            int y = tile.y;
            activeTiles.Add((x, y));

            UpdateIndicator(x, y, tile);
        }

        // Remove indicators for tiles that no longer have crops
        var toRemove = new List<(int, int)>();
        foreach (var kvp in activeIndicators)
        {
            if (!activeTiles.Contains(kvp.Key))
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var key in toRemove)
        {
            RemoveIndicator(key.Item1, key.Item2);
        }
    }

    /// <summary>
    /// Updates or creates an indicator for a specific crop tile.
    /// </summary>
    private void UpdateIndicator(int x, int y, TilemapDetail tile)
    {
        // Determine what indicator to show
        bool needsWater = tile.lastWateredAt == 0;
        bool isHarvestable = tile.growthStage == (int)GrowthStage.Harvestable;

        // Get or create indicator object
        if (!activeIndicators.TryGetValue((x, y), out GameObject indicator) || indicator == null)
        {
            indicator = CreateIndicator(x, y);
            activeIndicators[(x, y)] = indicator;
        }

        // Update visual
        var sr = indicator.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        if (isHarvestable)
        {
            // Show harvest-ready indicator
            sr.sprite = harvestIconSprite != null ? harvestIconSprite : CreateCircleSprite();
            sr.color = harvestIconSprite != null ? Color.white : harvestColor;
            float baseScale = harvestIconSprite != null ? 0.6f : 0.4f;
            float pulse = 1.0f + Mathf.Sin(Time.time * 3f) * 0.15f;
            indicator.transform.localScale = Vector3.one * baseScale * pulse;
        }
        else if (needsWater)
        {
            // Show needs-water indicator (custom thought-bubble icon or blue circle)
            sr.sprite = waterIconSprite != null ? waterIconSprite : CreateCircleSprite();
            sr.color = waterIconSprite != null ? Color.white : waterColor;
            indicator.transform.localScale = Vector3.one * (waterIconSprite != null ? 0.35f : 0.3f);
        }
        else
        {
            // Show growth progress (green, size based on stage)
            float progress = tile.growthStage / 4f; // 0-1 based on 5 stages
            sr.color = Color.Lerp(progressBgColor, progressFillColor, progress);
            indicator.transform.localScale = Vector3.one * (0.15f + progress * 0.25f);
        }
    }

    /// <summary>
    /// Creates a simple indicator sprite above a tile.
    /// Uses a built-in white square sprite as base.
    /// </summary>
    private GameObject CreateIndicator(int x, int y)
    {
        var go = new GameObject($"CropIndicator_{x}_{y}");
        go.transform.SetParent(transform);

        // Convert tile position to world position
        Vector3 worldPos = groundTilemap.CellToWorld(new Vector3Int(x, y, 0));
        worldPos += new Vector3(0.5f, yOffset, 0); // Center on tile + offset up
        go.transform.position = worldPos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.sortingOrder = 100; // Above most tilemap layers
        sr.color = progressFillColor;

        return go;
    }

    /// <summary>
    /// Removes an indicator for a tile.
    /// </summary>
    private void RemoveIndicator(int x, int y)
    {
        if (activeIndicators.TryGetValue((x, y), out GameObject indicator))
        {
            if (indicator != null) Destroy(indicator);
            activeIndicators.Remove((x, y));
        }
    }

    /// <summary>
    /// Creates a simple circle sprite at runtime (no asset needed).
    /// </summary>
    private Sprite CreateCircleSprite()
    {
        int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        float center = size / 2f;
        float radius = size / 2f - 1;

        for (int px = 0; px < size; px++)
        {
            for (int py = 0; py < size; py++)
            {
                float dist = Vector2.Distance(new Vector2(px, py), new Vector2(center, center));
                tex.SetPixel(px, py, dist <= radius ? Color.white : Color.clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }

    // ==================== EVENT HANDLERS ====================

    private void OnCropStageChanged(int x, int y, GrowthStage oldStage, GrowthStage newStage, CropDefinition cropDef)
    {
        // Immediately update indicator when stage changes
        var tile = tileMapManager?.GetTileAt(x, y);
        if (tile != null && tile.HasCrop)
        {
            UpdateIndicator(x, y, tile);
        }
    }

    private void OnCropHarvestable(int x, int y, CropDefinition cropDef)
    {
        string cropName = cropDef != null ? cropDef.cropName : "A crop";
        NotificationManager.Instance?.ShowNotification($"{cropName} is ready to harvest!", 2f);
    }

    void OnDestroy()
    {
        if (cropGrowthManager != null)
        {
            cropGrowthManager.OnCropStageChanged -= OnCropStageChanged;
            cropGrowthManager.OnCropHarvestable -= OnCropHarvestable;
        }

        // Cleanup all indicators
        foreach (var kvp in activeIndicators)
        {
            if (kvp.Value != null) Destroy(kvp.Value);
        }
        activeIndicators.Clear();
    }
}
