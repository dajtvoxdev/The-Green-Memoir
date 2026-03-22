using UnityEditor;
using UnityEngine;

/// <summary>
/// One-shot editor script to configure harvest icon sprite import
/// and wire it to CropIndicatorUI.harvestIconSprite.
/// </summary>
public class WireHarvestIcon
{
    [MenuItem("Tools/Wire Harvest Icon")]
    public static void Wire()
    {
        // 1. Configure sprite import settings
        string path = "Assets/Sprites/UI/icon_harvest_ready.png";
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"WireHarvestIcon: No TextureImporter at {path}");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 32;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        Debug.Log("WireHarvestIcon: Configured sprite import settings");

        // 2. Load the sprite
        Sprite harvestSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (harvestSprite == null)
        {
            Debug.LogError("WireHarvestIcon: Failed to load sprite!");
            return;
        }

        // 3. Find CropIndicatorUI in scene
        var indicator = Object.FindObjectOfType<CropIndicatorUI>();
        if (indicator == null)
        {
            Debug.LogError("WireHarvestIcon: CropIndicatorUI not found in scene!");
            return;
        }

        // 4. Wire the sprite via SerializedObject
        var so = new SerializedObject(indicator);
        var prop = so.FindProperty("harvestIconSprite");
        if (prop != null)
        {
            prop.objectReferenceValue = harvestSprite;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(indicator);
            Debug.Log($"WireHarvestIcon: Assigned '{harvestSprite.name}' to CropIndicatorUI.harvestIconSprite");
        }
        else
        {
            Debug.LogError("WireHarvestIcon: 'harvestIconSprite' property not found!");
        }
    }
}
