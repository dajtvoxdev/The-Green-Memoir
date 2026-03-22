using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class CreateBridgeTiles
{
    [MenuItem("Assets/Create/Moonlit Garden/Create Bridge Tiles")]
    static void CreateTiles()
    {
        // Create tiles directory if needed
        string tilesDir = "Assets/Tiles/VietnamFarm";
        if (!System.IO.Directory.Exists(tilesDir))
        {
            System.IO.Directory.CreateDirectory(tilesDir);
        }

        // Configure sprites as sprites first
        string[] tilePaths = new string[]
        {
            "Assets/Sprites/BridgeTileset/Bridge_Straight.png",
            "Assets/Sprites/BridgeTileset/Bridge_Corner.png",
            "Assets/Sprites/BridgeTileset/Bridge_End.png",
            "Assets/Sprites/BridgeTileset/Bridge_Railing.png"
        };

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

        // Create Tile assets
        CreateTile(tilePaths[0], "Bridge_Straight");
        CreateTile(tilePaths[1], "Bridge_Corner");
        CreateTile(tilePaths[2], "Bridge_End");
        CreateTile(tilePaths[3], "Bridge_Railing");

        Debug.Log("✅ Bridge tiles created!");
    }

    static void CreateTile(string spritePath, string tileName)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            Debug.LogError($"Could not load sprite: {spritePath}");
            return;
        }

        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.name = tileName;
        tile.colliderType = Tile.ColliderType.Sprite;

        string assetPath = $"Assets/Tiles/VietnamFarm/{tileName}.asset";
        AssetDatabase.CreateAsset(tile, assetPath);
        AssetDatabase.SaveAssets();
    }
}
