using UnityEngine;
using UnityEditor;

/// <summary>
/// Fixes fence PNG import settings to Sprite mode, then re-runs fence placement.
/// Run via: Tools > Moonlit Garden > Fix Fence Sprites & Rebuild
/// </summary>
public static class FixFenceSprites
{
    private static readonly string[] FENCE_SPRITES = new[]
    {
        "Assets/Sprites/Fences/fence_bamboo_horizontal.png",
        "Assets/Sprites/Fences/fence_bamboo_vertical.png",
        "Assets/Sprites/Fences/fence_bamboo_corner.png",
    };

    [MenuItem("Tools/Moonlit Garden/Fix Fence Sprites & Rebuild")]
    public static void FixAndRebuild()
    {
        bool changed = false;

        foreach (var path in FENCE_SPRITES)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning("[FixFence] Not found: " + path);
                continue;
            }

            bool needsFix = importer.textureType != TextureImporterType.Sprite
                         || importer.spriteImportMode != SpriteImportMode.Single
                         || importer.spritePixelsPerUnit != 32;

            if (needsFix)
            {
                importer.textureType       = TextureImporterType.Sprite;
                importer.spriteImportMode  = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode        = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                Debug.Log("[FixFence] Fixed: " + path);
                changed = true;
            }
            else
            {
                Debug.Log("[FixFence] Already correct: " + path);
            }
        }

        if (changed)
        {
            AssetDatabase.Refresh();
            Debug.Log("[FixFence] Sprite settings updated. Re-running fence placement...");
        }

        // Re-run fence placement with corrected sprites
        PlaceBambooFence.PlaceFence();
    }
}
