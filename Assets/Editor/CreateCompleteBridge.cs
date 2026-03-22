using UnityEngine;
using UnityEditor;

public class CreateCompleteBridgeTool
{
    [MenuItem("Tools/Moonlit Garden/Create Complete Bridge")]
    static void CreateBridge()
    {
        // Delete old bridge objects
        GameObject oldBridge = GameObject.Find("VietnameseBridge");
        if (oldBridge != null)
        {
            Object.DestroyImmediate(oldBridge);
        }
        
        GameObject bridgeTilemap = GameObject.Find("BridgeTilemap");
        if (bridgeTilemap != null)
        {
            Object.DestroyImmediate(bridgeTilemap);
        }

        // Configure the straight bridge sprite
        string spritePath = "Assets/Sprites/BridgeTileset/Bridge_Straight.png";
        TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 32;
            AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
        }

        Sprite bridgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (bridgeSprite == null)
        {
            Debug.LogError("Could not load bridge sprite!");
            return;
        }

        // Create the bridge GameObject
        GameObject bridge = new GameObject("VietnameseBambooBridge");
        
        // Add SpriteRenderer
        SpriteRenderer sr = bridge.AddComponent<SpriteRenderer>();
        sr.sprite = bridgeSprite;
        sr.sortingLayerName = "Objects";
        sr.sortingOrder = 10;
        
        // Scale to make a long bridge (6 tiles wide)
        bridge.transform.localScale = new Vector3(6f, 1.5f, 1f);
        
        // Position over the river
        bridge.transform.position = new Vector3(-3f, 1.5f, 0f);

        // Add BoxCollider2D for collision
        BoxCollider2D col = bridge.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.9f, 0.5f);
        col.offset = new Vector2(0, 0);

        // Create end posts (railing posts at both ends)
        GameObject leftPost = CreateRailingPost("LeftPost", new Vector3(-5.8f, 1.5f, 0f));
        GameObject rightPost = CreateRailingPost("RightPost", new Vector3(-0.2f, 1.5f, 0f));

        Debug.Log("✅ Vietnamese Bamboo Bridge created!");
        Selection.activeGameObject = bridge;
    }

    static GameObject CreateRailingPost(string name, Vector3 position)
    {
        string spritePath = "Assets/Sprites/BridgeTileset/Bridge_Railing.png";
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        
        GameObject post = new GameObject(name);
        post.transform.position = position;
        post.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        
        SpriteRenderer sr = post.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Objects";
        sr.sortingOrder = 11; // Higher than bridge
        
        return post;
    }
}
