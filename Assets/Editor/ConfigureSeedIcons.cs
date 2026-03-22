using UnityEngine;
using UnityEditor;

/// <summary>
/// Configures all seed icon PNGs in Assets/UI/ as proper Sprite imports,
/// then reassigns them to their corresponding ItemDefinition assets.
/// Menu: Tools > Configure Seed Icons
/// </summary>
public class ConfigureSeedIcons
{
    [MenuItem("Tools/Configure Seed Icons")]
    public static void Configure()
    {
        string[] guids = AssetDatabase.FindAssets("Seed_ t:Texture2D", new[] { "Assets/UI" });
        int configured = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.Contains("Seed_")) continue;

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            bool needsReimport = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                needsReimport = true;
            }
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                needsReimport = true;
            }
            if (importer.spritePixelsPerUnit != 32)
            {
                importer.spritePixelsPerUnit = 32;
                needsReimport = true;
            }
            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                needsReimport = true;
            }
            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                needsReimport = true;
            }

            if (needsReimport)
            {
                importer.SaveAndReimport();
                configured++;
                Debug.Log("[ConfigureSeedIcons] Configured: " + path);
            }
        }

        Debug.Log("[ConfigureSeedIcons] Configured " + configured + " seed icon sprites.");
    }

    [MenuItem("Tools/Reassign Seed Icons to Items")]
    public static void ReassignIcons()
    {
        string[] seedIds = {
            "seed_bean", "seed_corn", "seed_wheat", "seed_carrot", "seed_potato", "seed_onion",
            "seed_cucumber", "seed_tomato", "seed_cabbage", "seed_chili", "seed_eggplant",
            "seed_garlic", "seed_pumpkin", "seed_strawberry", "seed_watermelon",
            "seed_dragon_fruit", "seed_ginseng", "seed_rose"
        };

        int assigned = 0;
        foreach (string seedId in seedIds)
        {
            string itemPath = "Assets/Data/Items/" + seedId + ".asset";
            var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(itemPath);
            if (item == null)
            {
                Debug.LogWarning("[ConfigureSeedIcons] Item not found: " + itemPath);
                continue;
            }

            // Extract cropId from seedId (remove "seed_" prefix)
            string cropId = seedId.Substring(5);
            string iconName = "Seed_" + cropId.Substring(0, 1).ToUpper() + cropId.Substring(1);
            string iconPath = "Assets/UI/" + iconName + ".png";

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (sprite != null)
            {
                item.icon = sprite;
                EditorUtility.SetDirty(item);
                assigned++;
                Debug.Log("[ConfigureSeedIcons] Assigned icon to " + seedId + ": " + iconPath);
            }
            else
            {
                Debug.LogWarning("[ConfigureSeedIcons] Sprite not found: " + iconPath);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[ConfigureSeedIcons] Assigned icons to " + assigned + " seed items.");
    }
}
