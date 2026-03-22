using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Manages the tilemap system for the farm.
/// Phase 1 Optimization: Uses Dictionary for O(1) tile lookup instead of O(n) linear search.
/// Implements granular Firebase updates instead of full-document overwrite.
/// </summary>
public class TileMapManager : MonoBehaviour
{
    public Tilemap tm_Ground;
    public Tilemap tm_Grass;
    public Tilemap tm_Forest;

    public TileBase tb_Forest;
    public TileBase tb_TilledSoil;

    private FirebaseDatabaseManager databaseManager;
    private FirebaseTransactionManager transactionManager;
    private DatabaseReference reference;

    // ==================== PHASE 1: DICTIONARY CACHE FOR O(1) LOOKUP ====================
    // Key: (x, y) coordinate, Value: TilemapDetail
    // This replaces the O(n) linear search in SetStateForTilemapDetail
    private Dictionary<(int x, int y), TilemapDetail> tileCache;
    
    /// <summary>
    /// Gets the tile at the specified coordinates in O(1) time.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>The TilemapDetail at that position, or null if not found</returns>
    public TilemapDetail GetTileAt(int x, int y)
    {
        if (tileCache == null)
        {
            return null;
        }
        
        (int x, int y) key = (x, y);
        if (tileCache.TryGetValue(key, out TilemapDetail tile))
        {
            return tile;
        }
        return null;
    }
    
    /// <summary>
    /// Returns the entire tile cache dictionary.
    /// Used by CropIndicatorUI to scan all crop tiles.
    /// Phase 2.5D Fix (BP5): Supports visual feedback system.
    /// </summary>
    public Dictionary<(int x, int y), TilemapDetail> GetAllTiles()
    {
        return tileCache;
    }

    /// <summary>
    /// Initializes the tile cache from the current MapInGame data.
    /// Must be called after map data is loaded.
    /// </summary>
    private void InitializeTileCache()
    {
        tileCache = new Dictionary<(int, int), TilemapDetail>();
        
        if (LoadDataManager.userInGame?.MapInGame?.lstTilemapDetail == null)
        {
            Debug.LogWarning("TileMapManager: No map data to initialize cache!");
            return;
        }
        
        foreach (var tile in LoadDataManager.userInGame.MapInGame.lstTilemapDetail)
        {
            tileCache[(tile.x, tile.y)] = tile;
        }
        
        Debug.Log($"TileMapManager: Initialized tile cache with {tileCache.Count} tiles");
    }

    void Start()
    {
        databaseManager = GameObject.Find("DatabaseManager")?.GetComponent<FirebaseDatabaseManager>();
        transactionManager = GameObject.Find("DatabaseManager")?.GetComponent<FirebaseTransactionManager>();
        
        FirebaseApp app = FirebaseApp.DefaultInstance;
        reference = FirebaseDatabase.DefaultInstance.RootReference;

        // Null check for MapInGame (T10 fix)
        if (LoadDataManager.userInGame?.MapInGame?.lstTilemapDetail != null)
        {
            LoadMapForUser();
            InitializeTileCache(); // Build the dictionary cache
        }
        else
        {
            Debug.Log("TileMapManager: No existing map data, creating new map");
            WriteAllTileMapFireBase();
        }
    }

    /// <summary>
    /// Creates a new map with all tiles set to Grass state and saves to Firebase.
    /// </summary>
    public void WriteAllTileMapFireBase()
    {
        if (tm_Ground == null)
        {
            Debug.LogError("TileMapManager: tm_Ground is not assigned! Please assign it in the Inspector.");
            return;
        }
        
        List<TilemapDetail> tilemaps = new List<TilemapDetail>();

        for (int x = tm_Ground.cellBounds.min.x; x < tm_Ground.cellBounds.max.x; x++)
        {
            for (int y = tm_Ground.cellBounds.min.y; y < tm_Ground.cellBounds.max.y; y++)
            {
                TilemapDetail tm_detail = new TilemapDetail(x, y, TilemapState.Grass);
                tilemaps.Add(tm_detail);
            }
        }

        if (LoadDataManager.userInGame == null)
        {
            Debug.LogWarning("TileMapManager: userInGame is null, skipping map write");
            return;
        }

        LoadDataManager.userInGame.MapInGame = new Map(tilemaps);

        // Initialize cache after creating new map
        InitializeTileCache();

        if (LoadDataManager.firebaseUser != null)
        {
            if (LoadDataManager.Instance != null)
            {
                LoadDataManager.Instance.SaveUserInGame((success, error) =>
                {
                    if (!success)
                    {
                        Debug.LogError($"TileMapManager: Failed to save initialized map: {error}");
                    }
                });
            }
            else if (databaseManager != null)
            {
                databaseManager.WriteDatabase(
                    FirebaseUserPaths.GetUserProfilePath(LoadDataManager.firebaseUser.UserId),
                    LoadDataManager.userInGame.ToString());
            }
            else
            {
                Debug.LogError("TileMapManager: No Firebase save path is available for map initialization.");
            }
        }
    }

    /// <summary>
    /// Loads the user's map from cached data.
    /// </summary>
    public void LoadMapForUser()
    {
        Debug.Log("TileMapManager: Loading map for user");
        MapToUI(LoadDataManager.userInGame.MapInGame);
    }

    /// <summary>
    /// Renders a single tile based on its state.
    /// </summary>
    public void TilemapDetailToUI(TilemapDetail tilemapDetail)
    {
        Vector3Int cellPos = new Vector3Int(tilemapDetail.x, tilemapDetail.y, 0);

        if (tilemapDetail.tilemapState == TilemapState.Ground)
        {
            if (tm_Grass != null) tm_Grass.SetTile(cellPos, null);
            if (tm_Forest != null) tm_Forest.SetTile(cellPos, null);
        }
        else if (tilemapDetail.tilemapState == TilemapState.Tilled)
        {
            // Tilled farmland: clear forest, place tilled soil on grass layer
            if (tm_Forest != null) tm_Forest.SetTile(cellPos, null);
            if (tm_Grass != null && tb_TilledSoil != null)
                tm_Grass.SetTile(cellPos, tb_TilledSoil);
        }
        else if (tilemapDetail.tilemapState == TilemapState.Grass)
        {
            if (tm_Forest != null) tm_Forest.SetTile(cellPos, null);
        }
        else if (tilemapDetail.tilemapState == TilemapState.Forest)
        {
            if (tm_Grass != null) tm_Grass.SetTile(cellPos, null);
            if (tm_Forest != null) tm_Forest.SetTile(cellPos, tb_Forest);
        }
    }

    /// <summary>
    /// Renders all tiles from the map data.
    /// </summary>
    public void MapToUI(Map map)
    {
        if (map?.lstTilemapDetail == null)
        {
            Debug.LogError("TileMapManager: Map or tile list is null!");
            return;
        }
        
        Debug.Log($"TileMapManager: Loading {map.lstTilemapDetail.Count} tiles");
        
        foreach (var tile in map.lstTilemapDetail)
        {
            TilemapDetailToUI(tile);
        }
    }

    /// <summary>
    /// Sets the state for a tile at the specified coordinates.
    /// Phase 1: Uses O(1) dictionary lookup instead of O(n) linear search.
    /// Phase 1: Uses granular Firebase path update instead of full-document overwrite.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="state">New tile state</param>
    public void SetStateForTilemapDetail(int x, int y, TilemapState state)
    {
        // Null check for MapInGame (T10 fix)
        if (LoadDataManager.userInGame?.MapInGame?.lstTilemapDetail == null)
        {
            Debug.LogError("TileMapManager: MapInGame is null! Cannot set tile state.");
            return;
        }
        
        // O(1) lookup using dictionary cache
        TilemapDetail tile = GetTileAt(x, y);
        
        if (tile != null)
        {
            tile.tilemapState = state;
            
            // Update the visual tilemap
            TilemapDetailToUI(tile);
            
            // Phase 1: Granular Firebase update - only update this specific tile
            // Path: Users/{userId}/profile/MapInGame/lstTilemapDetail/{index}/tilemapState
            int index = LoadDataManager.userInGame.MapInGame.lstTilemapDetail.IndexOf(tile);
            if (index >= 0)
            {
                if (databaseManager == null)
                {
                    Debug.LogError("TileMapManager: databaseManager is null, cannot write tile state!");
                    return;
                }
                // Update the specific tile path in Firebase
                if (LoadDataManager.firebaseUser == null)
                {
                    Debug.LogWarning("TileMapManager: firebaseUser is null, skipping Firebase save.");
                    return;
                }
                string tilePath = $"{FirebaseUserPaths.GetProfileTilePath(LoadDataManager.firebaseUser.UserId, index)}/tilemapState";
                databaseManager.WriteDatabase(tilePath, JsonConvert.SerializeObject(state), (success, error) =>
                {
                    if (success)
                    {
                        Debug.Log($"TileMapManager: Saved tile ({x},{y}) state = {state}");
                    }
                    else
                    {
                        Debug.LogError($"TileMapManager: Failed to save tile state: {error}");
                    }
                });
            }
        }
        else
        {
            Debug.LogWarning($"TileMapManager: Tile at ({x},{y}) not found!");
        }
    }
    
    /// <summary>
    /// Async version of SetStateForTilemapDetail with granular Firebase update.
    /// Phase 1: Uses exponential backoff retry pattern.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="state">New tile state</param>
    /// <returns>Task that completes when the save is done</returns>
    public async Task SetTileStateAsync(int x, int y, TilemapState state)
    {
        // Null check for MapInGame (T10 fix)
        if (LoadDataManager.userInGame?.MapInGame?.lstTilemapDetail == null)
        {
            Debug.LogError("TileMapManager: MapInGame is null! Cannot set tile state.");
            return;
        }
        
        // O(1) lookup using dictionary cache
        TilemapDetail tile = GetTileAt(x, y);
        
        if (tile != null)
        {
            tile.tilemapState = state;
            TilemapDetailToUI(tile);
            
            // Find index for path construction
            int index = LoadDataManager.userInGame.MapInGame.lstTilemapDetail.IndexOf(tile);
            if (index >= 0)
            {
                if (LoadDataManager.firebaseUser == null)
                {
                    Debug.LogWarning("TileMapManager: firebaseUser is null, skipping async tile save.");
                    return;
                }

                string tilePath = $"{FirebaseUserPaths.GetProfileTilePath(LoadDataManager.firebaseUser.UserId, index)}/tilemapState";
                string stateJson = JsonConvert.SerializeObject(state);
                
                if (transactionManager != null)
                {
                    var response = await transactionManager.WriteWithRetry(tilePath, stateJson);
                    if (response.Success)
                    {
                        Debug.Log($"TileMapManager: Async save successful for tile ({x},{y})");
                    }
                    else
                    {
                        Debug.LogError($"TileMapManager: Async save failed: {response.ErrorMessage}");
                    }
                }
                else
                {
                    if (databaseManager != null)
                        databaseManager.WriteDatabase(tilePath, stateJson);
                    else
                        Debug.LogError("TileMapManager: databaseManager is null in SetTileStateAsync fallback!");
                }
            }
        }
    }
    
    /// <summary>
    /// Updates crop data on a tile and saves to Firebase.
    /// Phase 1: Granular update for crop persistence.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="cropId">Crop ID (null to clear)</param>
    /// <param name="growthStage">Current growth stage</param>
    public async Task UpdateCropDataAsync(int x, int y, string cropId, int growthStage = 0)
    {
        TilemapDetail tile = GetTileAt(x, y);
        
        if (tile != null)
        {
            // Only reset plantedAt when a NEW crop is being planted (cropId changes),
            // not on every growth stage update — resetting the timer would cause
            // the growth stage to oscillate back to Seed.
            bool isNewCrop = !string.IsNullOrEmpty(cropId) && tile.cropId != cropId;

            tile.cropId = cropId;
            tile.growthStage = growthStage;

            if (isNewCrop)
            {
                tile.plantedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
            else if (string.IsNullOrEmpty(cropId))
            {
                tile.plantedAt = 0;
            }
            
            // Find index for path construction
            int index = LoadDataManager.userInGame.MapInGame.lstTilemapDetail.IndexOf(tile);
            if (index >= 0)
            {
                if (LoadDataManager.firebaseUser == null)
                {
                    Debug.LogWarning("TileMapManager: firebaseUser is null, skipping crop save.");
                    return;
                }

                string tilePath = FirebaseUserPaths.GetProfileTilePath(LoadDataManager.firebaseUser.UserId, index);
                string tileJson = JsonConvert.SerializeObject(tile);
                
                if (transactionManager != null)
                {
                    await transactionManager.WriteWithRetry(tilePath, tileJson);
                }
                else if (databaseManager != null)
                {
                    databaseManager.WriteDatabase(tilePath, tileJson);
                }
            }
        }
    }
}
