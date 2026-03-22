using UnityEngine;
using UnityEditor;

public class ConfigureNewCropSprites
{
    [MenuItem("Tools/Configure New Crop Sprites")]
    public static void Configure()
    {
        string[] folders = { "Bean", "Corn", "Chili", "Eggplant", "Watermelon",
                             "Garlic", "Strawberry", "DragonFruit", "Ginseng", "Rose" };
        int count = 0;
        foreach (string folder in folders)
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites/Crops/" + folder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 32;
                    importer.filterMode = FilterMode.Point;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                    count++;
                }
            }
        }
        Debug.Log("[ConfigureSprites] Configured " + count + " sprites");
    }
}
