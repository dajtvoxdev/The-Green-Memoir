using UnityEngine;
using UnityEditor;

public class AssignBridgeSprite
{
    [InitializeOnLoadMethod]
    static void OnEditorLoad()
    {
        // Auto-assign when editor loads
        EditorApplication.delayCall += AssignSpriteToBridge;
    }

    [MenuItem("Tools/Moonlit Garden/Assign Bridge Sprite")]
    static void AssignSpriteToBridge()
    {
        GameObject bridge = GameObject.Find("VietnameseBambooBridge");
        if (bridge == null) return;

        SpriteRenderer sr = bridge.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // Configure the sprite import
        string path = "Assets/Sprites/BridgeTileset/Bridge_Straight.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 32;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        // Assign sprite
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null)
        {
            sr.sprite = sprite;
            sr.sortingLayerName = "Objects";
            sr.sortingOrder = 10;
            
            Debug.Log("✅ Bridge sprite assigned!");
        }
    }
}
