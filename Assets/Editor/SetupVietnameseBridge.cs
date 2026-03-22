using UnityEngine;
using UnityEditor;

public class SetupVietnameseBridgeTool
{
    [MenuItem("Tools/Moonlit Garden/Setup Vietnamese Bridge")]
    static void SetupBridge()
    {
        // Configure the texture as sprite first
        string path = "Assets/Sprites/VietnameseBridge.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
        
        // Find the bridge GameObject
        GameObject bridge = GameObject.Find("VietnameseBridge");
        if (bridge == null)
        {
            Debug.Log("Creating new VietnameseBridge GameObject...");
            bridge = new GameObject("VietnameseBridge");
            bridge.AddComponent<SpriteRenderer>();
            bridge.AddComponent<BoxCollider2D>();
        }

        // Load the sprite
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        
        if (sprite == null)
        {
            Debug.LogError("Could not load VietnameseBridge sprite! Make sure the file exists.");
            return;
        }

        // Configure SpriteRenderer
        SpriteRenderer sr = bridge.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = bridge.AddComponent<SpriteRenderer>();
        }
        
        sr.sprite = sprite;
        sr.sortingLayerName = "Objects";
        sr.sortingOrder = 5;

        // Configure BoxCollider2D
        BoxCollider2D col = bridge.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = bridge.AddComponent<BoxCollider2D>();
        }
        
        // Size collider to match sprite
        col.size = new Vector2(sprite.bounds.size.x, sprite.bounds.size.y * 0.3f);
        col.offset = new Vector2(0, sprite.bounds.size.y * 0.1f);

        // Scale and position over the river
        bridge.transform.localScale = new Vector3(2.5f, 2.0f, 1f);
        bridge.transform.position = new Vector3(-3f, 1f, 0f);

        Debug.Log("✅ Vietnamese Bridge setup complete!");
        Selection.activeGameObject = bridge;
    }
}
