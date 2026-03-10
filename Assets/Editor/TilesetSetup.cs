using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public static class TilesetSetup
{
    [MenuItem("Tools/Setup Vietnamese Farm Tilesets")]
    public static void SetupTilesets()
    {
        string[] tilesetNames = { "tileset_path", "tileset_farmsoil", "tileset_rice", "tileset_canal" };
        string tilesetDir = "Assets/Sprites/Tilesets";
        string tileDir = "Assets/Tiles/VietnamFarm";

        if (!AssetDatabase.IsValidFolder("Assets/Tiles"))
            AssetDatabase.CreateFolder("Assets", "Tiles");
        if (!AssetDatabase.IsValidFolder(tileDir))
            AssetDatabase.CreateFolder("Assets/Tiles", "VietnamFarm");

        foreach (string tilesetName in tilesetNames)
        {
            string texPath = $"{tilesetDir}/{tilesetName}.png";
            TextureImporter importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"Texture not found: {texPath}");
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 32;

            List<SpriteMetaData> spriteSheet = new List<SpriteMetaData>();
            for (int i = 0; i < 16; i++)
            {
                int col = i % 4;
                int row = i / 4;
                SpriteMetaData smd = new SpriteMetaData();
                smd.name = $"{tilesetName}_{i}";
                smd.rect = new Rect(col * 32, (3 - row) * 32, 32, 32);
                smd.alignment = (int)SpriteAlignment.Center;
                smd.pivot = new Vector2(0.5f, 0.5f);
                spriteSheet.Add(smd);
            }
            importer.spritesheet = spriteSheet.ToArray();
            importer.SaveAndReimport();

            string subDir = $"{tileDir}/{tilesetName}";
            if (!AssetDatabase.IsValidFolder(subDir))
                AssetDatabase.CreateFolder(tileDir, tilesetName);

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texPath);
            int tileCount = 0;
            foreach (Object asset in assets)
            {
                if (asset is Sprite sprite && sprite.name.Contains("_"))
                {
                    string tilePath = $"{subDir}/{sprite.name}.asset";
                    if (AssetDatabase.LoadAssetAtPath<Tile>(tilePath) != null)
                        continue;

                    Tile tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = sprite;
                    tile.colliderType = Tile.ColliderType.None;
                    AssetDatabase.CreateAsset(tile, tilePath);
                    tileCount++;
                }
            }
            Debug.Log($"TilesetSetup: Created {tileCount} tiles for {tilesetName}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("TilesetSetup: Vietnamese Farm tileset setup complete!");
    }
}
