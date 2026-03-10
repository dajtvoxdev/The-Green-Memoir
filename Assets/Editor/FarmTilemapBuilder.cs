using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Editor tool that rebuilds the PlayScene tilemaps using HarvestQuestFree sprites.
/// Creates a Vietnamese-style farming layout: green, lush, with clear farm plots.
///
/// Run via menu: Tools > Moonlit Garden > Rebuild Farm Tilemaps
/// </summary>
public class FarmTilemapBuilder : EditorWindow
{
    private const string HQ_FOLDER = "Assets/HarvestQuestFree";
    private const string TILE_OUTPUT_FOLDER = "Assets/Data/FarmTiles";
    private const int MAP_HALF_W = 15; // -15 to +15 = 31 wide
    private const int MAP_HALF_H = 10; // -10 to +10 = 21 tall

    // HarvestQuestFree sprite sheet paths
    private const string GRASS_PATH   = HQ_FOLDER + "/GrassFree.png";
    private const string PATH_PATH    = HQ_FOLDER + "/Path1Spring.png";
    private const string FENCE_PATH   = HQ_FOLDER + "/FencesFree.png";
    private const string WATER_PATH   = HQ_FOLDER + "/WaterFree.png";
    private const string OBJECTS_PATH = HQ_FOLDER + "/ObjectsFree.png";

    // Also keep old tileset for fallback crops/decoration
    private const string OLD_TILESET_PATH = "Assets/Farming Asset Pack/farming-tileset.png";

    [MenuItem("Tools/Moonlit Garden/Rebuild Farm Tilemaps")]
    public static void RebuildTilemaps()
    {
        Debug.Log("FarmTilemapBuilder: Starting rebuild with HarvestQuestFree assets...");

        // Step 1: Import all HQ sprite sheets with correct settings
        ImportAllSpriteSheets();

        // Step 2: Create tile assets from all sprites
        var tiles = CreateAllTileAssets();
        if (tiles == null || tiles.Count == 0)
        {
            Debug.LogError("FarmTilemapBuilder: No tiles created!");
            return;
        }
        Debug.Log($"FarmTilemapBuilder: Total {tiles.Count} tile assets ready");

        // Step 3: Find tilemaps
        var grid = GameObject.Find("Grid");
        if (grid == null)
        {
            Debug.LogError("FarmTilemapBuilder: Grid not found in scene!");
            return;
        }

        Tilemap groundMap = null, grassMap = null, forestMap = null;
        foreach (var tm in grid.GetComponentsInChildren<Tilemap>())
        {
            string n = tm.gameObject.name.ToLower();
            if (n.Contains("ground")) groundMap = tm;
            else if (n.Contains("grass")) grassMap = tm;
            else if (n.Contains("forest")) forestMap = tm;
        }

        if (groundMap == null || grassMap == null)
        {
            Debug.LogError("FarmTilemapBuilder: Could not find Ground/Grass tilemaps!");
            return;
        }

        // Step 4: Clear and repaint
        groundMap.ClearAllTiles();
        grassMap.ClearAllTiles();
        if (forestMap != null) forestMap.ClearAllTiles();

        PaintGround(groundMap, tiles);
        PaintFarmAndPaths(grassMap, tiles);
        PaintDecorations(forestMap, tiles);

        // Step 5: Sorting orders
        SetSortingOrder(groundMap, 0);
        SetSortingOrder(grassMap, 1);
        if (forestMap != null) SetSortingOrder(forestMap, 2);

        // Mark dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("FarmTilemapBuilder: Rebuild complete!");
    }

    static void SetSortingOrder(Tilemap tm, int order)
    {
        var r = tm.GetComponent<TilemapRenderer>();
        if (r != null) r.sortingOrder = order;
    }

    // ─── IMPORT SPRITE SHEETS ───────────────────────────────────────

    static void ImportAllSpriteSheets()
    {
        // GrassFree: 48x16 → 3 tiles of 16x16
        ImportSpriteSheet(GRASS_PATH, 16, 16, 16, "GrassFree");
        // Path1Spring: 192x64 → 12x4 = 48 tiles? Actually it's a 9-slice type.
        // 192/16=12 cols, 64/16=4 rows
        ImportSpriteSheet(PATH_PATH, 16, 16, 16, "Path1Spring");
        // FencesFree: 64x64 → 4x4 = 16 tiles
        ImportSpriteSheet(FENCE_PATH, 16, 16, 16, "FencesFree");
        // WaterFree: 48x80 → 3x5 = 15 tiles
        ImportSpriteSheet(WATER_PATH, 16, 16, 16, "WaterFree");
        // ObjectsFree: 384x384 → 24x24 = 576 tiles at 16x16
        ImportSpriteSheet(OBJECTS_PATH, 16, 16, 16, "ObjectsFree");
        // Old tileset (keep for extra variety)
        ImportSpriteSheet(OLD_TILESET_PATH, 32, 32, 32, "farming-tileset");
    }

    static void ImportSpriteSheet(string path, int cellW, int cellH, int ppu, string prefix)
    {
        TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) { Debug.LogWarning($"FarmTilemapBuilder: Cannot find {path}"); return; }

        bool changed = false;

        if (imp.spritePixelsPerUnit != ppu) { imp.spritePixelsPerUnit = ppu; changed = true; }
        if (imp.filterMode != FilterMode.Point) { imp.filterMode = FilterMode.Point; changed = true; }
        if (imp.textureCompression != TextureImporterCompression.Uncompressed)
        { imp.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
        if (imp.textureType != TextureImporterType.Sprite)
        { imp.textureType = TextureImporterType.Sprite; changed = true; }

        // Read texture dimensions
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) return;
        int texW = tex.width, texH = tex.height;
        int cols = texW / cellW, rows = texH / cellH;
        int totalSprites = cols * rows;

        if (imp.spriteImportMode != SpriteImportMode.Multiple || changed)
        {
            imp.spriteImportMode = SpriteImportMode.Multiple;

            var metaData = new SpriteMetaData[totalSprites];
            int idx = 0;
            for (int row = rows - 1; row >= 0; row--)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (idx >= totalSprites) break;
                    metaData[idx] = new SpriteMetaData
                    {
                        name = $"{prefix}_{idx}",
                        rect = new Rect(col * cellW, row * cellH, cellW, cellH),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    };
                    idx++;
                }
            }

#pragma warning disable CS0618
            imp.spritesheet = metaData;
#pragma warning restore CS0618
            changed = true;
        }

        if (changed)
        {
            imp.SaveAndReimport();
            Debug.Log($"FarmTilemapBuilder: Imported {prefix} ({cols}x{rows} = {totalSprites} sprites, PPU={ppu})");
        }
    }

    // ─── CREATE TILE ASSETS ─────────────────────────────────────────

    static Dictionary<string, Tile> CreateAllTileAssets()
    {
        if (!AssetDatabase.IsValidFolder(TILE_OUTPUT_FOLDER))
        {
            string parent = Path.GetDirectoryName(TILE_OUTPUT_FOLDER).Replace("\\", "/");
            if (!AssetDatabase.IsValidFolder(parent))
                AssetDatabase.CreateFolder("Assets", "Data");
            AssetDatabase.CreateFolder(parent, "FarmTiles");
        }

        var allTiles = new Dictionary<string, Tile>();

        // Load sprites from each sheet
        LoadSpritesAsTiles(GRASS_PATH, allTiles);
        LoadSpritesAsTiles(PATH_PATH, allTiles);
        LoadSpritesAsTiles(FENCE_PATH, allTiles);
        LoadSpritesAsTiles(WATER_PATH, allTiles);
        LoadSpritesAsTiles(OBJECTS_PATH, allTiles);
        LoadSpritesAsTiles(OLD_TILESET_PATH, allTiles);

        AssetDatabase.SaveAssets();
        return allTiles;
    }

    static void LoadSpritesAsTiles(string sheetPath, Dictionary<string, Tile> tiles)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
        foreach (var obj in assets)
        {
            if (obj is Sprite sprite)
            {
                string tilePath = $"{TILE_OUTPUT_FOLDER}/{sprite.name}.asset";
                Tile existing = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);

                if (existing != null)
                {
                    existing.sprite = sprite;
                    EditorUtility.SetDirty(existing);
                    tiles[sprite.name] = existing;
                }
                else
                {
                    Tile newTile = ScriptableObject.CreateInstance<Tile>();
                    newTile.sprite = sprite;
                    newTile.color = Color.white;
                    AssetDatabase.CreateAsset(newTile, tilePath);
                    tiles[sprite.name] = newTile;
                }
            }
        }
    }

    static Tile T(Dictionary<string, Tile> tiles, string name)
    {
        return tiles.ContainsKey(name) ? tiles[name] : null;
    }

    // ─── PAINT GROUND (Base green grass) ────────────────────────────

    static void PaintGround(Tilemap map, Dictionary<string, Tile> tiles)
    {
        // GrassFree: 3 tiles (GrassFree_0, _1, _2)
        var grassTiles = new List<Tile>();
        for (int i = 0; i < 3; i++)
        {
            var t = T(tiles, $"GrassFree_{i}");
            if (t != null) grassTiles.Add(t);
        }

        // Fallback to old grass if HQ grass not available
        if (grassTiles.Count == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                var t = T(tiles, $"farming-tileset_{i}");
                if (t != null) grassTiles.Add(t);
            }
        }

        if (grassTiles.Count == 0)
        {
            Debug.LogError("FarmTilemapBuilder: No grass tiles found!");
            return;
        }

        // Fill entire map with weighted random grass
        Random.InitState(42);
        for (int x = -MAP_HALF_W; x <= MAP_HALF_W; x++)
        {
            for (int y = -MAP_HALF_H; y <= MAP_HALF_H; y++)
            {
                float r = Random.value;
                int idx = grassTiles.Count >= 3
                    ? (r < 0.65f ? 0 : (r < 0.88f ? 1 : 2))
                    : (r < 0.7f ? 0 : Mathf.Min(1, grassTiles.Count - 1));
                map.SetTile(new Vector3Int(x, y, 0), grassTiles[idx]);
            }
        }

        Debug.Log("FarmTilemapBuilder: Ground painted (green grass base)");
    }

    // ─── PAINT FARM PLOTS, PATHS, FENCES, WATER ────────────────────

    static void PaintFarmAndPaths(Tilemap map, Dictionary<string, Tile> tiles)
    {
        // Path1Spring (192x64, 12 cols x 4 rows at 16x16):
        // Visual layout: [left-edge 2cols | center-fill 6cols | right-edge 4cols]
        // Row 0 indices 0-11, Row 1 indices 12-23, Row 2 indices 24-35, Row 3 indices 36-47
        // Center fill (uniform brown): cols 3-8 → indices 3,4,5,6,7,8 in row 0
        // Use ONE tile for uniform soil across all plots
        Tile soilTile = T(tiles, "Path1Spring_5") // center of the brown fill area
                     ?? T(tiles, "Path1Spring_4")
                     ?? T(tiles, "Path1Spring_6");

        // Path tiles — use Path1Spring center variant for dirt paths
        // (farming-tileset_3 was actually a building tile from bottom row!)
        Tile pathTile = T(tiles, "Path1Spring_4")  // brown dirt center
                     ?? T(tiles, "Path1Spring_6")
                     ?? soilTile;

        // FencesFree (64x64, 4x4 at 16x16):
        // Visual: 3 vertical columns of fence posts with horizontal beams
        // Each column is 1 tile wide with post on top/bottom
        Tile fenceTile = T(tiles, "FencesFree_0") ?? T(tiles, "FencesFree_1");

        // Water tiles
        Tile waterCenter = T(tiles, "WaterFree_7") ?? T(tiles, "WaterFree_4");
        Tile waterEdge   = T(tiles, "WaterFree_1") ?? T(tiles, "WaterFree_0");

        // ═══ 4 FARM PLOTS (2x2 grid) ═══
        // Each plot: 5 wide x 3 tall, uniform soil
        int[][] plotCoords = new int[][]
        {
            new int[] { -7, 3 },  // top-left
            new int[] {  1, 3 },  // top-right
            new int[] { -7, -3 }, // bottom-left
            new int[] {  1, -3 }, // bottom-right
        };

        if (soilTile != null)
        {
            foreach (var plot in plotCoords)
            {
                int px = plot[0], py = plot[1];
                for (int x = px; x < px + 5; x++)
                {
                    for (int y = py; y < py + 3; y++)
                    {
                        map.SetTile(new Vector3Int(x, y, 0), soilTile);
                    }
                }
            }
        }

        // ═══ PATHS (clean single-tile dirt) ═══
        if (pathTile != null)
        {
            // Horizontal main path: y=0 to 1
            for (int x = -10; x <= 8; x++)
            {
                map.SetTile(new Vector3Int(x, 0, 0), pathTile);
                map.SetTile(new Vector3Int(x, 1, 0), pathTile);
            }
            // Vertical center path: x=-1 to 0
            for (int y = -6; y <= 7; y++)
            {
                map.SetTile(new Vector3Int(-1, y, 0), pathTile);
                map.SetTile(new Vector3Int(0, y, 0), pathTile);
            }
            // South exit
            for (int y = -MAP_HALF_H; y <= -6; y++)
            {
                map.SetTile(new Vector3Int(-1, y, 0), pathTile);
                map.SetTile(new Vector3Int(0, y, 0), pathTile);
            }
            // North exit
            for (int y = 7; y <= MAP_HALF_H; y++)
            {
                map.SetTile(new Vector3Int(-1, y, 0), pathTile);
                map.SetTile(new Vector3Int(0, y, 0), pathTile);
            }
        }

        // ═══ FENCE around farm area ═══
        int fL = -10, fR = 8, fB = -6, fT = 7;
        if (fenceTile != null)
        {
            // Top & bottom
            for (int x = fL; x <= fR; x++)
            {
                map.SetTile(new Vector3Int(x, fT, 0), fenceTile);
                map.SetTile(new Vector3Int(x, fB, 0), fenceTile);
            }
            // Left & right
            for (int y = fB; y <= fT; y++)
            {
                map.SetTile(new Vector3Int(fL, y, 0), fenceTile);
                map.SetTile(new Vector3Int(fR, y, 0), fenceTile);
            }
            // Gate openings (south & north, 2 tiles wide)
            map.SetTile(new Vector3Int(-1, fB, 0), null);
            map.SetTile(new Vector3Int(0, fB, 0), null);
            map.SetTile(new Vector3Int(-1, fT, 0), null);
            map.SetTile(new Vector3Int(0, fT, 0), null);
        }

        // ═══ SMALL POND (ao nước — top right outside fence) ═══
        if (waterCenter != null)
        {
            for (int x = 10; x <= 13; x++)
            {
                for (int y = 7; y <= 9; y++)
                {
                    bool edge = (x == 10 || x == 13 || y == 7 || y == 9);
                    map.SetTile(new Vector3Int(x, y, 0), edge ? (waterEdge ?? waterCenter) : waterCenter);
                }
            }
        }

        Debug.Log("FarmTilemapBuilder: Farm plots, paths, fences, and pond painted");
    }

    // ─── PAINT DECORATIONS ──────────────────────────────────────────
    // IMPORTANT: Use old farming-tileset (32x32, proper single-tile sprites)
    // for trees/bushes/flowers/crops. ObjectsFree has multi-tile sprites that
    // look fragmented when used as individual 16x16 tiles.

    static void PaintDecorations(Tilemap map, Dictionary<string, Tile> tiles)
    {
        if (map == null) return;

        // farming-tileset.png: 13 cols × 12 rows (416x384 @ 32x32), sliced bottom-to-top
        // Visual row 0 (top) = idx 143-155: terrain patches, fish
        // Visual row 1 = idx 130-142: fence, tall crops, round trees, big green bushes
        // Visual row 2 = idx 117-129: plants, seedlings, trees, green hedges
        // Visual row 3 = idx 104-116: vegetables (corn, tomato, carrot, etc.)
        // Visual rows 4-11 = idx 0-103: bags, tools, buildings (DO NOT USE)

        // Trees: round green trees + big green bushes (visual rows 1-2)
        var treeTiles = new List<Tile>();
        foreach (int i in new[] { 136, 123, 138, 139, 125, 126 })
        {
            var t = T(tiles, $"farming-tileset_{i}");
            if (t != null) treeTiles.Add(t);
        }

        // Bushes: green vegetation/hedges (visual rows 1-2)
        var bushTiles = new List<Tile>();
        foreach (int i in new[] { 140, 141, 142, 127, 128, 129 })
        {
            var t = T(tiles, $"farming-tileset_{i}");
            if (t != null) bushTiles.Add(t);
        }

        // Flowers / small plants: seedlings, sprouts (visual row 2)
        var flowerTiles = new List<Tile>();
        foreach (int i in new[] { 119, 120, 121, 122, 134, 135 })
        {
            var t = T(tiles, $"farming-tileset_{i}");
            if (t != null) flowerTiles.Add(t);
        }

        // Crops: tall wheat/corn + vegetables (visual rows 1, 3)
        var cropTiles = new List<Tile>();
        foreach (int i in new[] { 132, 133, 117, 118, 104 })
        {
            var t = T(tiles, $"farming-tileset_{i}");
            if (t != null) cropTiles.Add(t);
        }

        Random.InitState(888);

        // ═══ DENSE TREE BORDER (2 layers) ═══
        if (treeTiles.Count > 0)
        {
            for (int x = -MAP_HALF_W; x <= MAP_HALF_W; x++)
            {
                // Top edge (skip path exit at x=-1,0)
                if (x != -1 && x != 0)
                {
                    if (Random.value > 0.1f)
                        map.SetTile(new Vector3Int(x, MAP_HALF_H, 0), treeTiles[Random.Range(0, treeTiles.Count)]);
                    if (Random.value > 0.3f)
                        map.SetTile(new Vector3Int(x, MAP_HALF_H - 1, 0), treeTiles[Random.Range(0, treeTiles.Count)]);
                }
                // Bottom edge (skip path exit)
                if (x != -1 && x != 0)
                {
                    if (Random.value > 0.1f)
                        map.SetTile(new Vector3Int(x, -MAP_HALF_H, 0), treeTiles[Random.Range(0, treeTiles.Count)]);
                    if (Random.value > 0.3f)
                        map.SetTile(new Vector3Int(x, -MAP_HALF_H + 1, 0), treeTiles[Random.Range(0, treeTiles.Count)]);
                }
            }
            // Left & right sides
            for (int y = -MAP_HALF_H + 2; y <= MAP_HALF_H - 2; y++)
            {
                if (Random.value > 0.15f)
                    map.SetTile(new Vector3Int(-MAP_HALF_W, y, 0), treeTiles[Random.Range(0, treeTiles.Count)]);
                if (Random.value > 0.35f)
                    map.SetTile(new Vector3Int(-MAP_HALF_W + 1, y, 0), treeTiles[Random.Range(0, treeTiles.Count)]);
                if (Random.value > 0.15f)
                    map.SetTile(new Vector3Int(MAP_HALF_W, y, 0), treeTiles[Random.Range(0, treeTiles.Count)]);
                if (Random.value > 0.35f)
                    map.SetTile(new Vector3Int(MAP_HALF_W - 1, y, 0), treeTiles[Random.Range(0, treeTiles.Count)]);
            }
        }

        // ═══ CROPS inside farm plots ═══
        if (cropTiles.Count > 0)
        {
            int[][] plots = new int[][]
            {
                new int[] { -7, 3 }, new int[] { 1, 3 },
                new int[] { -7, -3 }, new int[] { 1, -3 },
            };
            for (int p = 0; p < plots.Length; p++)
            {
                int px = plots[p][0], py = plots[p][1];
                Tile crop = cropTiles[p % cropTiles.Count];
                for (int x = px; x < px + 5; x++)
                {
                    for (int y = py; y < py + 3; y++)
                    {
                        if (Random.value > 0.2f)
                            map.SetTile(new Vector3Int(x, y, 0), crop);
                    }
                }
            }
        }

        // ═══ BUSHES in open areas (outside fence) ═══
        if (bushTiles.Count > 0)
        {
            for (int i = 0; i < 18; i++)
            {
                int x = Random.Range(-MAP_HALF_W + 3, MAP_HALF_W - 3);
                int y = Random.Range(-MAP_HALF_H + 3, MAP_HALF_H - 3);
                if (x >= -11 && x <= 9 && y >= -7 && y <= 8) continue; // skip farm+fence area
                map.SetTile(new Vector3Int(x, y, 0), bushTiles[Random.Range(0, bushTiles.Count)]);
            }
        }

        // ═══ FLOWERS along paths ═══
        if (flowerTiles.Count > 0)
        {
            for (int y = -MAP_HALF_H + 2; y <= -7; y += 2)
            {
                if (Random.value > 0.4f)
                    map.SetTile(new Vector3Int(-2, y, 0), flowerTiles[Random.Range(0, flowerTiles.Count)]);
                if (Random.value > 0.4f)
                    map.SetTile(new Vector3Int(1, y, 0), flowerTiles[Random.Range(0, flowerTiles.Count)]);
            }
            for (int i = 0; i < 12; i++)
            {
                int x = Random.Range(-MAP_HALF_W + 3, MAP_HALF_W - 3);
                int y = Random.Range(-MAP_HALF_H + 3, MAP_HALF_H - 3);
                if (x >= -11 && x <= 9 && y >= -7 && y <= 8) continue;
                map.SetTile(new Vector3Int(x, y, 0), flowerTiles[Random.Range(0, flowerTiles.Count)]);
            }
        }

        Debug.Log($"FarmTilemapBuilder: Decorations painted (trees:{treeTiles.Count}, bushes:{bushTiles.Count}, crops:{cropTiles.Count}, flowers:{flowerTiles.Count})");
    }
}
