using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class RebuildPlaySceneTilemap
{
    private const string GeneratedTileFolder = "Assets/Data/FarmTilesGenerated";

    private const int MinX = -15;
    private const int MaxX = 15;
    private const int MinY = -10;
    private const int MaxY = 10;

    [MenuItem("Tools/RebuildPlaySceneTilemap")]
    public static void Apply()
    {
        Grid grid = UnityEngine.Object.FindFirstObjectByType<Grid>();
        if (grid == null)
        {
            Debug.LogError("RebuildPlaySceneTilemap: Grid not found.");
            return;
        }

        Tilemap tmGround = FindOrCreateTilemap(grid.transform, "Ground");
        Tilemap tmGrass = FindOrCreateTilemap(grid.transform, "Grass");
        Tilemap tmForest = FindOrCreateTilemap(grid.transform, "Forest");
        Tilemap tmDecor = FindOrCreateTilemap(grid.transform, "Decorations");
        Tilemap tmTrees = FindOrCreateTilemap(grid.transform, "Trees");
        Tilemap tmBuildings = FindOrCreateTilemap(grid.transform, "Buildings");
        Tilemap tmProps = FindOrCreateTilemap(grid.transform, "Props");

        SetSortingOrder(tmGround, 0);
        SetSortingOrder(tmGrass, 1);
        SetSortingOrder(tmDecor, 2);
        SetSortingOrder(tmForest, 3); // Keep for crop visuals at runtime.
        SetSortingOrder(tmTrees, 4);
        SetSortingOrder(tmBuildings, 5);
        SetSortingOrder(tmProps, 6);

        tmGround.ClearAllTiles();
        tmGrass.ClearAllTiles();
        tmDecor.ClearAllTiles();
        tmForest.ClearAllTiles();
        tmTrees.ClearAllTiles();
        tmBuildings.ClearAllTiles();
        tmProps.ClearAllTiles();

        // Core visual tiles.
        TileBase baseGrass = LoadDataTile("GrassFree_0");
        TileBase altGrass = LoadDataTile("GrassFree_1") ?? baseGrass;
        TileBase[] grassVariants = FilterNonNull(baseGrass, altGrass);

        TileBase pathTile = LoadDataTile("Path1Spring_5") ?? LoadDataTile("Path1Spring_4");
        TileBase soilTile = LoadDataTile("Path1Spring_6")
                            ?? LoadDataTile("Path1Spring_5")
                            ?? pathTile;
        TileBase waterCenter = LoadDataTile("WaterFree_7") ?? LoadDataTile("WaterFree_4");
        TileBase waterEdge = LoadDataTile("WaterFree_1") ?? waterCenter;
        TileBase fenceTile = LoadDataTile("FencesFree_0") ?? LoadDataTile("FencesFree_1");

        // Functional farm mask tile (invisible) for till/plant logic.
        TileBase farmMaskTile = CreateOrUpdateMaskTile("FarmPlantableMask");

        // Elv tree + grass object sprites.
        TileBase treeTile = CreateOrUpdateSingleSpriteTile(
            "Assets/ElvGames/Farm Game Assets/Objects/FG_Tree_Spring.png",
            "FG_Tree_Spring",
            32);
        TileBase bushTile = CreateOrUpdateSingleSpriteTile(
            "Assets/ElvGames/Farm Game Assets/Objects/FG_Grass_Spring.png",
            "FG_Grass_Spring",
            32);

        // Buildings + decorative animals.
        TileBase houseTile = CreateOrUpdateSpriteTileFromMultiple(
            "Assets/Farming Asset Pack/farming-houses.png",
            "farming-houses_0",
            "farming-houses_0",
            32);
        TileBase shopTile = CreateOrUpdateSpriteTileFromMultiple(
            "Assets/Farming Asset Pack/farming-houses.png",
            "farming-houses_3",
            "farming-houses_3",
            32);
        TileBase chickenTile = CreateOrUpdateSpriteTileFromMultiple(
            "Assets/Farming Asset Pack/farming-chicken.png",
            "farming-chicken_0",
            "farming-chicken_0",
            32);
        TileBase cowTile = CreateOrUpdateSpriteTileFromMultiple(
            "Assets/Farming Asset Pack/farming-cow.png",
            "farming-cow_0",
            "farming-cow_0",
            32);

        // Props (used as crate/barrel/tool markers).
        TileBase crateTile = CreateOrUpdateSingleSpriteTile(
            "Assets/ElvGames/Farm Game Assets/Objects/FG_Treasure_1.png",
            "FG_Treasure_1",
            32);
        TileBase barrelTile = CreateOrUpdateSingleSpriteTile(
            "Assets/ElvGames/Farm Game Assets/Objects/FG_Treasure_2.png",
            "FG_Treasure_2",
            32);
        TileBase toolTile = CreateOrUpdateSingleSpriteTile(
            "Assets/ElvGames/Farm Game Assets/Objects/FG_Treasure_Small_1.png",
            "FG_Treasure_Small_1",
            32);

        TileBase fallback = grassVariants.Length > 0 ? grassVariants[0] : (soilTile ?? pathTile);
        if (fallback == null)
        {
            Debug.LogError("RebuildPlaySceneTilemap: Missing core tile assets.");
            return;
        }

        System.Random rng = new System.Random(220326);

        // 1) Ground fill (mostly green, subtle variation).
        for (int x = MinX; x <= MaxX; x++)
        {
            for (int y = MinY; y <= MaxY; y++)
            {
                TileBase tile = fallback;
                if (grassVariants.Length > 1 && rng.NextDouble() < 0.18)
                {
                    tile = grassVariants[1];
                }
                tmGround.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        // 2) Main path network (narrower than before).
        if (pathTile != null)
        {
            DrawRect(tmGround, 0, MinY, 1, 19, pathTile);    // Center vertical.
            DrawRect(tmGround, -13, 1, 13, 1, pathTile);     // Branch to pen.
            DrawRect(tmGround, 0, 1, 12, 1, pathTile);       // Branch to shop.
            DrawRect(tmGround, -4, -8, 8, 1, pathTile);      // House yard path.
        }

        // 3) Central farm field visuals + invisible plantable mask.
        RectInt[] farmPlots = new RectInt[]
        {
            new RectInt(-7, 0, 3, 2),
            new RectInt(-2, 0, 3, 2),
            new RectInt(3, 0, 3, 2),
            new RectInt(-7, 3, 3, 2),
            new RectInt(-2, 3, 3, 2),
            new RectInt(3, 3, 3, 2)
        };

        foreach (RectInt plot in farmPlots)
        {
            DrawRect(tmGround, plot.x, plot.y, plot.width, plot.height, soilTile ?? fallback);
            if (farmMaskTile != null)
            {
                DrawRect(tmGrass, plot.x, plot.y, plot.width, plot.height, farmMaskTile);
            }
        }

        // 4) Small pond (upper-right).
        for (int x = 10; x <= 13; x++)
        {
            for (int y = 6; y <= 8; y++)
            {
                bool isEdge = x == 10 || x == 13 || y == 6 || y == 8;
                tmGround.SetTile(new Vector3Int(x, y, 0), isEdge ? waterEdge : waterCenter);
            }
        }

        // 5) Trees border (top + side boundaries).
        if (treeTile != null)
        {
            for (int x = MinX; x <= MaxX; x++)
            {
                if (Math.Abs(x) > 1)
                {
                    if (rng.NextDouble() < 0.95) tmTrees.SetTile(new Vector3Int(x, 10, 0), treeTile);
                    if (rng.NextDouble() < 0.8) tmTrees.SetTile(new Vector3Int(x, 9, 0), treeTile);
                }
            }
            for (int y = -8; y <= 8; y++)
            {
                if (rng.NextDouble() < 0.85) tmTrees.SetTile(new Vector3Int(-15, y, 0), treeTile);
                if (rng.NextDouble() < 0.55) tmTrees.SetTile(new Vector3Int(-14, y, 0), treeTile);
                if (rng.NextDouble() < 0.85) tmTrees.SetTile(new Vector3Int(15, y, 0), treeTile);
                if (rng.NextDouble() < 0.55) tmTrees.SetTile(new Vector3Int(14, y, 0), treeTile);
            }
        }

        // 6) Left animal pen (fence).
        int penL = -14, penR = -9, penB = -1, penT = 4;
        if (fenceTile != null)
        {
            for (int x = penL; x <= penR; x++)
            {
                tmProps.SetTile(new Vector3Int(x, penB, 0), fenceTile);
                tmProps.SetTile(new Vector3Int(x, penT, 0), fenceTile);
            }
            for (int y = penB; y <= penT; y++)
            {
                tmProps.SetTile(new Vector3Int(penL, y, 0), fenceTile);
                tmProps.SetTile(new Vector3Int(penR, y, 0), fenceTile);
            }
            tmProps.SetTile(new Vector3Int(-11, penB, 0), null);
            tmProps.SetTile(new Vector3Int(-10, penB, 0), null);
        }

        // 7) Animals in pen.
        if (chickenTile != null)
        {
            tmProps.SetTile(new Vector3Int(-13, 1, 0), chickenTile);
            tmProps.SetTile(new Vector3Int(-11, 3, 0), chickenTile);
        }
        if (cowTile != null)
        {
            tmProps.SetTile(new Vector3Int(-12, 2, 0), cowTile);
        }

        // 8) Buildings.
        if (houseTile != null) tmBuildings.SetTile(new Vector3Int(-3, -8, 0), houseTile);
        if (shopTile != null) tmBuildings.SetTile(new Vector3Int(11, 4, 0), shopTile);

        // 9) Props near house/shop.
        if (crateTile != null) tmProps.SetTile(new Vector3Int(-2, -7, 0), crateTile);
        if (barrelTile != null) tmProps.SetTile(new Vector3Int(-1, -7, 0), barrelTile);
        if (toolTile != null) tmProps.SetTile(new Vector3Int(-4, -7, 0), toolTile);
        if (crateTile != null) tmProps.SetTile(new Vector3Int(10, 3, 0), crateTile);
        if (barrelTile != null) tmProps.SetTile(new Vector3Int(12, 3, 0), barrelTile);

        // 10) Decoration scatter.
        if (bushTile != null)
        {
            Scatter(tmDecor, bushTile, rng, 28);
        }

        // Keep center farm + player route readable.
        ClearRect(tmDecor, -8, -1, 17, 8);

        // Ensure farming controller uses the same functional farm mask tile.
        PlayerFarmController farmCtrl = UnityEngine.Object.FindFirstObjectByType<PlayerFarmController>();
        if (farmCtrl != null && farmMaskTile != null)
        {
            farmCtrl.tb_Grass = farmMaskTile;
            if (farmCtrl.tb_Ground == null)
            {
                farmCtrl.tb_Ground = fallback;
            }
            EditorUtility.SetDirty(farmCtrl);
        }

        // Keep camera/player in current expected framing.
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0f, -1.5f, -10f);
            EditorUtility.SetDirty(cam);
        }

        GameObject player = GameObject.Find("Player") ?? GameObject.Find("Player ");
        if (player != null)
        {
            player.transform.localScale = new Vector3(1.35f, 1.35f, 1f);
            player.transform.position = new Vector3(0f, -1f, 0f);
            EditorUtility.SetDirty(player);
        }

        EditorUtility.SetDirty(tmGround);
        EditorUtility.SetDirty(tmGrass);
        EditorUtility.SetDirty(tmDecor);
        EditorUtility.SetDirty(tmForest);
        EditorUtility.SetDirty(tmTrees);
        EditorUtility.SetDirty(tmBuildings);
        EditorUtility.SetDirty(tmProps);

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("RebuildPlaySceneTilemap: Peaceful farm layout applied.");
    }

    private static Tilemap FindOrCreateTilemap(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child == null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            child = go.transform;
        }

        Tilemap tilemap = child.GetComponent<Tilemap>();
        if (tilemap == null) tilemap = child.gameObject.AddComponent<Tilemap>();
        if (child.GetComponent<TilemapRenderer>() == null) child.gameObject.AddComponent<TilemapRenderer>();
        return tilemap;
    }

    private static void EnsureSingleSpriteSettings(string texturePath, int ppu)
    {
        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer == null) return;

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
        if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; changed = true; }
        if (Math.Abs(importer.spritePixelsPerUnit - ppu) > 0.01f) { importer.spritePixelsPerUnit = ppu; changed = true; }
        if (importer.filterMode != FilterMode.Point) { importer.filterMode = FilterMode.Point; changed = true; }
        if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }

        if (changed) importer.SaveAndReimport();
    }

    private static void EnsureMultipleSpriteSettings(string texturePath, int ppu)
    {
        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer == null) return;

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
        if (importer.spriteImportMode != SpriteImportMode.Multiple) { importer.spriteImportMode = SpriteImportMode.Multiple; changed = true; }
        if (Math.Abs(importer.spritePixelsPerUnit - ppu) > 0.01f) { importer.spritePixelsPerUnit = ppu; changed = true; }
        if (importer.filterMode != FilterMode.Point) { importer.filterMode = FilterMode.Point; changed = true; }
        if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }

        if (changed) importer.SaveAndReimport();
    }

    private static TileBase CreateOrUpdateSingleSpriteTile(string texturePath, string tileName, int ppu)
    {
        EnsureFolder(GeneratedTileFolder);
        EnsureSingleSpriteSettings(texturePath, ppu);

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
        if (sprite == null)
        {
            sprite = AssetDatabase.LoadAllAssetsAtPath(texturePath).OfType<Sprite>().FirstOrDefault();
        }
        if (sprite == null) return null;

        string tilePath = $"{GeneratedTileFolder}/{tileName}.asset";
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.color = Color.white;
            AssetDatabase.CreateAsset(tile, tilePath);
        }
        else if (tile.sprite != sprite)
        {
            tile.sprite = sprite;
            EditorUtility.SetDirty(tile);
        }
        return tile;
    }

    private static TileBase CreateOrUpdateSpriteTileFromMultiple(string texturePath, string spriteName, string tileName, int ppu)
    {
        EnsureFolder(GeneratedTileFolder);
        EnsureMultipleSpriteSettings(texturePath, ppu);

        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(texturePath)
            .OfType<Sprite>()
            .FirstOrDefault(s => s.name == spriteName);
        if (sprite == null) return null;

        string tilePath = $"{GeneratedTileFolder}/{tileName}.asset";
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.color = Color.white;
            AssetDatabase.CreateAsset(tile, tilePath);
        }
        else if (tile.sprite != sprite)
        {
            tile.sprite = sprite;
            EditorUtility.SetDirty(tile);
        }
        return tile;
    }

    private static TileBase CreateOrUpdateMaskTile(string tileName)
    {
        EnsureFolder(GeneratedTileFolder);
        string tilePath = $"{GeneratedTileFolder}/{tileName}.asset";
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = null;
            tile.color = Color.white;
            AssetDatabase.CreateAsset(tile, tilePath);
        }
        else if (tile.sprite != null)
        {
            tile.sprite = null;
            EditorUtility.SetDirty(tile);
        }

        return tile;
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath)) return;

        string[] parts = folderPath.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }

    private static TileBase LoadDataTile(string tileName)
    {
        return AssetDatabase.LoadAssetAtPath<TileBase>($"Assets/Data/FarmTiles/{tileName}.asset");
    }

    private static TileBase[] FilterNonNull(params TileBase[] tiles)
    {
        List<TileBase> result = new List<TileBase>();
        foreach (TileBase t in tiles) if (t != null) result.Add(t);
        return result.ToArray();
    }

    private static void SetSortingOrder(Tilemap tm, int order)
    {
        TilemapRenderer renderer = tm.GetComponent<TilemapRenderer>();
        if (renderer != null) renderer.sortingOrder = order;
    }

    private static void DrawRect(Tilemap map, int x, int y, int width, int height, TileBase[] variants, System.Random rng)
    {
        if (variants == null || variants.Length == 0) return;

        for (int px = x; px < x + width; px++)
        {
            for (int py = y; py < y + height; py++)
            {
                map.SetTile(new Vector3Int(px, py, 0), variants[rng.Next(variants.Length)]);
            }
        }
    }

    private static void DrawRect(Tilemap map, int x, int y, int width, int height, TileBase tile)
    {
        if (tile == null) return;
        for (int px = x; px < x + width; px++)
        {
            for (int py = y; py < y + height; py++)
            {
                map.SetTile(new Vector3Int(px, py, 0), tile);
            }
        }
    }

    private static void Scatter(Tilemap map, TileBase tile, System.Random rng, int count)
    {
        if (map == null || tile == null) return;

        int placed = 0;
        int attempts = 0;
        while (placed < count && attempts < count * 20)
        {
            attempts++;
            int x = rng.Next(MinX + 1, MaxX);
            int y = rng.Next(MinY + 1, MaxY);

            // Keep key gameplay lanes clear.
            if (x >= -8 && x <= 8 && y >= 1 && y <= 8) continue;
            if (x >= -2 && x <= 1 && y >= MinY && y <= 8) continue;

            Vector3Int pos = new Vector3Int(x, y, 0);
            if (map.GetTile(pos) != null) continue;

            map.SetTile(pos, tile);
            placed++;
        }
    }

    private static void ClearRect(Tilemap map, int x, int y, int width, int height)
    {
        if (map == null) return;

        for (int px = x; px < x + width; px++)
        {
            for (int py = y; py < y + height; py++)
            {
                map.SetTile(new Vector3Int(px, py, 0), null);
            }
        }
    }
}
