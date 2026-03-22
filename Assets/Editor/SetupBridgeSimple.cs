using UnityEngine;
using UnityEditor;

public class SetupBridgeSimple
{
    [MenuItem("Tools/Moonlit Garden/Setup Bridge Sprite")]
    static void SetupBridge()
    {
        // Find the bridge
        GameObject bridge = GameObject.Find("VietnameseBambooBridge");
        if (bridge == null)
        {
            Debug.LogError("VietnameseBambooBridge not found! Please create it first.");
            return;
        }

        // Configure sprite import
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

        // Load and assign sprite
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogError("Could not load sprite!");
            return;
        }

        SpriteRenderer sr = bridge.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = bridge.AddComponent<SpriteRenderer>();
        }
        
        sr.sprite = sprite;
        sr.sortingLayerName = "Objects";
        sr.sortingOrder = 10;
        
        // Scale to make a long bridge (5 tiles wide, 1.5 height)
        bridge.transform.localScale = new Vector3(5f, 1.5f, 1f);
        bridge.transform.position = new Vector3(-3f, 1.5f, 0f);

        // Configure collider
        BoxCollider2D col = bridge.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = bridge.AddComponent<BoxCollider2D>();
        }
        col.size = new Vector2(0.9f, 0.3f);
        col.offset = new Vector2(0, 0);

        Debug.Log("✅ Bridge sprite setup complete!");
        Selection.activeGameObject = bridge;
        EditorGUIUtility.PingObject(bridge);
    }
}
