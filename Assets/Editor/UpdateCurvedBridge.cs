using UnityEngine;
using UnityEditor;

public class UpdateCurvedBridge
{
    [MenuItem("Tools/Moonlit Garden/Update Curved Bridge")]
    static void UpdateBridge()
    {
        // Configure new sprite
        string path = "Assets/Sprites/VietnameseBambooBridge_Curved.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 64; // Match tile size
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        // Find and update bridge
        GameObject bridge = GameObject.Find("VietnameseBambooBridge");
        if (bridge == null)
        {
            // Create new if not exists
            bridge = new GameObject("VietnameseBambooBridge");
            bridge.AddComponent<SpriteRenderer>();
            bridge.AddComponent<BoxCollider2D>();
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogError("Could not load curved bridge sprite!");
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
        
        // Adjust scale for the new 64x64 tile
        bridge.transform.localScale = new Vector3(3f, 3f, 1f);
        bridge.transform.position = new Vector3(-3f, 1.5f, 0f);

        // Update collider
        BoxCollider2D col = bridge.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = bridge.AddComponent<BoxCollider2D>();
        }
        col.size = new Vector2(0.8f, 0.4f);
        col.offset = new Vector2(0, 0);

        Debug.Log("✅ Curved bridge updated!");
        Selection.activeGameObject = bridge;
    }
}
