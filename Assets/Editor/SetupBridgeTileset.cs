using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class SetupBridgeTilesetTool
{
    [MenuItem("Tools/Moonlit Garden/Setup Bridge Tileset")]
    static void SetupBridge()
    {
        // Configure all bridge sprites
        string[] tilePaths = new string[]
        {
            "Assets/Sprites/BridgeTileset/Bridge_Straight.png",
            "Assets/Sprites/BridgeTileset/Bridge_Corner.png",
            "Assets/Sprites/BridgeTileset/Bridge_End.png",
            "Assets/Sprites/BridgeTileset/Bridge_Railing.png"
        };

        // Import all as sprites
        foreach (var path in tilePaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        // Find or create BridgeTilemap
        GameObject grid = GameObject.Find("Grid");
        if (grid == null)
        {
            Debug.LogError("Grid not found!");
            return;
        }

        // Find BridgeTilemap or create it
        Transform bridgeTilemapTrans = grid.transform.Find("BridgeTilemap");
        Tilemap bridgeTilemap;
        TilemapRenderer bridgeRenderer;
        
        if (bridgeTilemapTrans == null)
        {
            GameObject bridgeGO = new GameObject("BridgeTilemap");
            bridgeGO.transform.SetParent(grid.transform);
            bridgeTilemap = bridgeGO.AddComponent<Tilemap>();
            bridgeRenderer = bridgeGO.AddComponent<TilemapRenderer>();
            bridgeRenderer.sortingLayerName = "Objects";
            bridgeRenderer.sortingOrder = 10;
        }
        else
        {
            bridgeTilemap = bridgeTilemapTrans.GetComponent<Tilemap>();
            bridgeRenderer = bridgeTilemapTrans.GetComponent<TilemapRenderer>();
        }

        // Load sprites
        Sprite straightSprite = AssetDatabase.LoadAssetAtPath<Sprite>(tilePaths[0]);
        Sprite endSprite = AssetDatabase.LoadAssetAtPath<Sprite>(tilePaths[2]);
        
        if (straightSprite == null)
        {
            Debug.LogError("Could not load bridge sprites!");
            return;
        }

        // Create tiles
        Tile straightTile = CreateTile(straightSprite, "Bridge_Straight");
        Tile endTile = CreateTile(endSprite, "Bridge_End");

        // Clear existing bridge tiles
        bridgeTilemap.ClearAllTiles();

        // Place bridge over the river (horizontal bridge at y=1, from x=-5 to x=-1)
        // River seems to be around x=-3 to x=-1 based on previous screenshots
        
        // Left end of bridge
        bridgeTilemap.SetTile(new Vector3Int(-6, 1, 0), endTile);
        
        // Middle straight sections
        bridgeTilemap.SetTile(new Vector3Int(-5, 1, 0), straightTile);
        bridgeTilemap.SetTile(new Vector3Int(-4, 1, 0), straightTile);
        bridgeTilemap.SetTile(new Vector3Int(-3, 1, 0), straightTile);
        bridgeTilemap.SetTile(new Vector3Int(-2, 1, 0), straightTile);
        
        // Right end of bridge
        bridgeTilemap.SetTile(new Vector3Int(-1, 1, 0), endTile);

        // Also add a vertical bridge above (at x=-3, y from 2 to 5)
        // Start
        bridgeTilemap.SetTile(new Vector3Int(-3, 2, 0), endTile);
        bridgeTilemap.SetTile(new Vector3Int(-3, 3, 0), straightTile);
        bridgeTilemap.SetTile(new Vector3Int(-3, 4, 0), straightTile);
        bridgeTilemap.SetTile(new Vector3Int(-3, 5, 0), endTile);

        Debug.Log("✅ Bridge tileset setup complete!");
        
        // Delete old VietnameseBridge GameObject if exists
        GameObject oldBridge = GameObject.Find("VietnameseBridge");
        if (oldBridge != null)
        {
            Object.DestroyImmediate(oldBridge);
            Debug.Log("Deleted old VietnameseBridge GameObject");
        }

        // Select the tilemap
        Selection.activeGameObject = bridgeTilemap.gameObject;
    }

    static Tile CreateTile(Sprite sprite, string name)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.name = name;
        tile.colliderType = Tile.ColliderType.Sprite;
        
        // Save to asset
        string path = $"Assets/Tiles/VietnamFarm/{name}.asset";
        AssetDatabase.CreateAsset(tile, path);
        AssetDatabase.SaveAssets();
        
        return tile;
    }
}
