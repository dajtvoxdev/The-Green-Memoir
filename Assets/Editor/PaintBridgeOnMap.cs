using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class PaintBridgeOnMap : EditorWindow
{
    [MenuItem("Tools/Moonlit Garden/Paint Bridge on Map")]
    static void PaintBridge()
    {
        // Find BridgeTilemap
        GameObject bridgeTilemapGO = GameObject.Find("BridgeTilemap");
        if (bridgeTilemapGO == null)
        {
            Debug.LogError("BridgeTilemap not found! Run Setup Bridge Tileset first.");
            return;
        }

        Tilemap bridgeTilemap = bridgeTilemapGO.GetComponent<Tilemap>();
        if (bridgeTilemap == null)
        {
            Debug.LogError("BridgeTilemap has no Tilemap component!");
            return;
        }

        // Load or create tiles
        Tile straightTile = LoadOrCreateTile("Bridge_Straight", "Assets/Sprites/BridgeTileset/Bridge_Straight.png");
        Tile endTile = LoadOrCreateTile("Bridge_End", "Assets/Sprites/BridgeTileset/Bridge_End.png");

        if (straightTile == null || endTile == null)
        {
            Debug.LogError("Could not create bridge tiles!");
            return;
        }

        // Clear existing
        bridgeTilemap.ClearAllTiles();

        // Paint horizontal bridge crossing the river
        // Based on screenshot, river is around x=-3 to x=-1, y around 1-2
        
        // Horizontal bridge at y=1, from x=-5 to x=-1
        // Left end
        bridgeTilemap.SetTile(new Vector3Int(-5, 1, 0), endTile);
        // Middle
        bridgeTilemap.SetTile(new Vector3Int(-4, 1, 0), straightTile);
        bridgeTilemap.SetTile(new Vector3Int(-3, 1, 0), straightTile);
        bridgeTilemap.SetTile(new Vector3Int(-2, 1, 0), straightTile);
        // Right end
        bridgeTilemap.SetTile(new Vector3Int(-1, 1, 0), endTile);

        Debug.Log("✅ Bridge painted on map!");
        
        // Select the tilemap
        Selection.activeGameObject = bridgeTilemapGO;
        EditorGUIUtility.PingObject(bridgeTilemapGO);
    }

    static Tile LoadOrCreateTile(string tileName, string spritePath)
    {
        // Try to load existing
        string tilePath = $"Assets/Tiles/VietnamFarm/{tileName}.asset";
        Tile existingTile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (existingTile != null)
        {
            return existingTile;
        }

        // Create new
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            // Configure texture first
            TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
            }
            
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        }

        if (sprite == null)
        {
            Debug.LogError($"Could not load sprite: {spritePath}");
            return null;
        }

        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.name = tileName;
        tile.colliderType = Tile.ColliderType.Sprite;

        AssetDatabase.CreateAsset(tile, tilePath);
        AssetDatabase.SaveAssets();

        return tile;
    }
}
