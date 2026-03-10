using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

/// <summary>
/// Paints a Vietnamese countryside farm layout using:
///   1. Corner-based Wang autotiling (matching PixelLab's 16-tile Wang system)
///   2. Dual-layer rendering: Ground fill + Terrain autotiles
///   3. Clean rectangular zones for organized grid appearance
///
/// PixelLab Wang system: each tile has 4 corners (NW, NE, SW, SE),
/// each corner is "lower" (zone terrain) or "upper" (grass background).
/// Corner is "lower" when BOTH adjacent cardinal neighbors belong to the zone.
/// </summary>
public static class PaintFarmLayout
{
    enum Zone { Grass, Path, FarmSoil, Rice, Water }

    const int MIN_X = -20, MAX_X = 20, MIN_Y = -14, MAX_Y = 14;
    const int W = MAX_X - MIN_X + 1;  // 41
    const int H = MAX_Y - MIN_Y + 1;  // 29

    // ═══════════════════════════════════════════════════════════
    // CORNER-BASED WANG TILE MAPPING (derived from PixelLab metadata)
    //
    // Corner mask bits: NW=8, NE=4, SW=2, SE=1
    // A corner is "lower" (=1) when BOTH cardinal neighbors sharing
    // that corner belong to the same zone.
    //
    // From PixelLab metadata (tileset_path, tileset_canal, etc.):
    //   wang_X has corners → corner_mask = (15 - X)
    //   wang_X is at a specific bounding_box position in the 128x128 PNG
    //   TilesetSetup.cs slices sprites as index 0-15 (left-to-right, top-to-bottom
    //   with Unity's bottom-left origin via: Rect(col*32, (3-row)*32, 32, 32))
    //
    // Final mapping: corner_mask → sprite index
    // ═══════════════════════════════════════════════════════════
    static readonly int[] CORNER_MASK_TO_TILE = {
    //  mask:  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15
               0,  1, 12, 15,  4, 13,  2,  9,  3,  8,  7, 14,  5,  6, 11, 10
    };
    // mask  0 = all corners upper (isolated cell)     → wang_15 → sprite 0
    // mask 15 = all corners lower (full interior)     → wang_0  → sprite 10

    // Grass color for camera background (matched to PixelLab grass tile)
    static readonly Color32 GRASS_COLOR = new Color32( 38, 140, 20, 255);

    [MenuItem("Tools/Paint Vietnamese Farm Layout")]
    public static void PaintLayout()
    {
        var ground  = FindOrCreateTilemap("Ground", 0);
        var terrain = FindOrCreateTilemap("Terrain", 1);
        if (ground == null)
        {
            Debug.LogError("PaintFarmLayout: Cannot find or create Grid + tilemaps!");
            return;
        }

        string td = "Assets/Tiles/VietnamFarm";
        var pathT  = LoadTiles($"{td}/tileset_path",     "tileset_path");
        var soilT  = LoadTiles($"{td}/tileset_farmsoil", "tileset_farmsoil");
        var riceT  = LoadTiles($"{td}/tileset_rice",     "tileset_rice");
        var waterT = LoadTiles($"{td}/tileset_canal",    "tileset_canal");

        if (pathT == null || soilT == null || riceT == null || waterT == null)
        {
            Debug.LogError("PaintFarmLayout: One or more tile sets missing! Run Tools > Setup Vietnamese Farm Tilesets first.");
            return;
        }

        // Use actual PixelLab tiles for ground fill instead of synthetic colors:
        //   wang_15 (all corners upper = full grass) → sprite index 0
        //   wang_0  (all corners lower = full interior) → sprite index 10
        int grassIdx    = CORNER_MASK_TO_TILE[0];   // mask 0 → tile 0 (wang_15)
        int interiorIdx = CORNER_MASK_TO_TILE[15];   // mask 15 → tile 10 (wang_0)

        Tile grassFill = pathT[grassIdx];             // full grass from path tileset
        Tile pathFill  = pathT[interiorIdx];           // full path interior
        Tile soilFill  = soilT[interiorIdx];           // full soil interior
        Tile riceFill  = riceT[interiorIdx];           // full rice interior
        Tile waterFill = waterT[interiorIdx];          // full canal water interior

        // Build zone grid
        var grid = new Zone[W, H];
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
                grid[x, y] = Zone.Grass;

        DesignRectangularLayout(grid);

        // Clear old tiles
        ground.ClearAllTiles();
        terrain.ClearAllTiles();
        Debug.Log("PaintFarmLayout: Cleared Ground and Terrain layers");

        // Set camera background to grass color
        var cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(GRASS_COLOR.r / 255f, GRASS_COLOR.g / 255f, GRASS_COLOR.b / 255f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        // ═══ GROUND LAYER: Solid fill tiles ═══
        // Extend grass well beyond map bounds to prevent camera background
        // showing through (darkened by URP 2D lighting).
        int PAD = 15;
        for (int wx = MIN_X - PAD; wx <= MAX_X + PAD; wx++)
        {
            for (int wy = MIN_Y - PAD; wy <= MAX_Y + PAD; wy++)
            {
                int gx = wx - MIN_X;
                int gy = wy - MIN_Y;
                Tile fill = grassFill;
                if (gx >= 0 && gx < W && gy >= 0 && gy < H)
                {
                    switch (grid[gx, gy])
                    {
                        case Zone.Path:     fill = pathFill;  break;
                        case Zone.FarmSoil: fill = soilFill;  break;
                        case Zone.Rice:     fill = riceFill;  break;
                        case Zone.Water:    fill = waterFill;  break;
                    }
                }
                ground.SetTile(new Vector3Int(wx, wy, 0), fill);
            }
        }
        Debug.Log("PaintFarmLayout: Ground layer filled");

        // ═══ TERRAIN LAYER: Corner-based Wang autotiles ═══
        // Paint all zone cells (borders get transition tiles, interiors get wang_0).
        // Order: paint from bottom layer up so higher-priority zones overlay.
        CornerWangPaint(terrain, grid, Zone.Water,    waterT);
        CornerWangPaint(terrain, grid, Zone.Rice,     riceT);
        CornerWangPaint(terrain, grid, Zone.FarmSoil, soilT);
        CornerWangPaint(terrain, grid, Zone.Path,     pathT);

        // Mark scene dirty
        EditorUtility.SetDirty(ground);
        EditorUtility.SetDirty(terrain);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("PaintFarmLayout: Vietnamese countryside farm layout complete!");
    }

    // ═══════════════════════════════════════════════════════════
    // LAYOUT DESIGN — Clean rectangular zones
    // Inspired by Vietnamese countryside: houses, paths, rice
    // paddies, farm plots, pond — all in clear rectangular grid.
    //
    // World coords: x ∈ [-20,+20], y ∈ [-14,+14]
    // Grid coords:  gx = wx - MIN_X, gy = wy - MIN_Y
    // ═══════════════════════════════════════════════════════════
    static void DesignRectangularLayout(Zone[,] g)
    {
        // ─── Ao làng (Village pond) — bottom-left ───────────
        FillRect(g, Zone.Water, -18, -12, -11, -6);

        // ─── Ruộng lúa (Rice paddy) — top-right, large ─────
        FillRect(g, Zone.Rice, 8, 4, 19, 12);

        // ─── Đất trồng (Farm plots) — 4 rectangular beds ───
        // Plot 1: bottom center-left
        FillRect(g, Zone.FarmSoil, -8, -12, -3, -7);
        // Plot 2: bottom center
        FillRect(g, Zone.FarmSoil, 0, -12, 5, -7);
        // Plot 3: bottom right
        FillRect(g, Zone.FarmSoil, 8, -12, 13, -7);
        // Plot 4: top-left garden
        FillRect(g, Zone.FarmSoil, -18, 2, -12, 7);

        // ─── Sân nhà (House yard) — top-center ─────────────
        FillRect(g, Zone.Path, -8, 7, 5, 13);

        // ─── Đường làng (Village paths) — clear rectangles ──
        // Main horizontal path across the map (3 tiles wide)
        FillRect(g, Zone.Path, -18, -3, 18, -1);
        // Vertical path: main road → house yard
        FillRect(g, Zone.Path, -2, -1, 0, 7);
        // Branch: path → pond (connects main path to pond area)
        FillRect(g, Zone.Path, -14, -6, -12, -3);
        // Branch: path → farm plot 3 (right side)
        FillRect(g, Zone.Path, 6, -6, 7, -3);
        // Branch: path → rice paddy
        FillRect(g, Zone.Path, 6, -1, 7, 4);

        // ─── Mương nước (Irrigation canal around rice) ──────
        // Left canal edge
        FillRect(g, Zone.Water, 6, 4, 7, 12);
        // Bottom canal edge
        FillRect(g, Zone.Water, 6, 3, 19, 4);
    }

    // ═══════════════════════════════════════════════════════════
    // CORNER-BASED WANG AUTOTILE PAINTER
    //
    // For each zone cell, compute 4-corner mask:
    //   NW corner = lower if (W neighbor AND N neighbor are same zone)
    //   NE corner = lower if (N neighbor AND E neighbor are same zone)
    //   SW corner = lower if (W neighbor AND S neighbor are same zone)
    //   SE corner = lower if (S neighbor AND E neighbor are same zone)
    //
    // Bits: NW=8, NE=4, SW=2, SE=1
    // ═══════════════════════════════════════════════════════════
    static void CornerWangPaint(Tilemap tm, Zone[,] g, Zone zone, Tile[] tiles)
    {
        int totalCount = 0;
        int borderCount = 0;

        for (int gx = 0; gx < W; gx++)
        {
            for (int gy = 0; gy < H; gy++)
            {
                if (g[gx, gy] != zone) continue;
                totalCount++;

                // Check cardinal neighbors
                bool hasN = (gy + 1 < H)  && g[gx,     gy + 1] == zone;
                bool hasS = (gy - 1 >= 0) && g[gx,     gy - 1] == zone;
                bool hasE = (gx + 1 < W)  && g[gx + 1, gy]     == zone;
                bool hasW = (gx - 1 >= 0) && g[gx - 1, gy]     == zone;

                // Compute corner mask
                int mask = 0;
                if (hasN && hasW) mask |= 8;  // NW corner
                if (hasN && hasE) mask |= 4;  // NE corner
                if (hasS && hasW) mask |= 2;  // SW corner
                if (hasS && hasE) mask |= 1;  // SE corner

                // Paint ALL cells including interiors (mask=15 → wang_0).
                // Ground layer has the base fill; terrain layer adds PixelLab's
                // actual textured tile on top for consistent pixel art look.
                int tileIdx = CORNER_MASK_TO_TILE[mask];
                tm.SetTile(new Vector3Int(gx + MIN_X, gy + MIN_Y, 0), tiles[tileIdx]);
                if (mask < 15) borderCount++;
            }
        }
        Debug.Log($"PaintFarmLayout: {zone} — {totalCount} cells, {borderCount} border tiles painted");
    }

    // ═══════════════════════════════════════════════════════════
    // SHAPE PRIMITIVES — Axis-aligned rectangles only
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Fill a rectangle in world coordinates (inclusive bounds).
    /// </summary>
    static void FillRect(Zone[,] g, Zone z, int wxMin, int wyMin, int wxMax, int wyMax)
    {
        int gxMin = Mathf.Max(0,     wxMin - MIN_X);
        int gxMax = Mathf.Min(W - 1, wxMax - MIN_X);
        int gyMin = Mathf.Max(0,     wyMin - MIN_Y);
        int gyMax = Mathf.Min(H - 1, wyMax - MIN_Y);

        for (int gx = gxMin; gx <= gxMax; gx++)
            for (int gy = gyMin; gy <= gyMax; gy++)
                g[gx, gy] = z;
    }

    // ═══════════════════════════════════════════════════════════
    // UNITY HELPERS
    // ═══════════════════════════════════════════════════════════
    static Tilemap FindOrCreateTilemap(string name, int sortOrder)
    {
        Grid grid = Object.FindFirstObjectByType<Grid>();
        if (grid == null)
        {
            Debug.LogError($"PaintFarmLayout: No Grid found in scene! Create a Grid GameObject first.");
            return null;
        }

        // Try to find existing
        foreach (var tm in grid.GetComponentsInChildren<Tilemap>())
        {
            if (tm.gameObject.name == name) return tm;
        }

        // Create new tilemap layer
        var go = new GameObject(name);
        go.transform.SetParent(grid.transform);
        go.transform.localPosition = Vector3.zero;
        var tilemap = go.AddComponent<Tilemap>();
        var renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = sortOrder;
        Debug.Log($"PaintFarmLayout: Created tilemap layer '{name}' (sortOrder={sortOrder})");
        return tilemap;
    }

    static Tile[] LoadTiles(string dir, string prefix)
    {
        var tiles = new Tile[16];
        for (int i = 0; i < 16; i++)
        {
            tiles[i] = AssetDatabase.LoadAssetAtPath<Tile>($"{dir}/{prefix}_{i}.asset");
            if (tiles[i] == null)
            {
                Debug.LogWarning($"PaintFarmLayout: Missing tile {dir}/{prefix}_{i}.asset");
                return null;
            }
        }
        return tiles;
    }
}
