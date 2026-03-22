using UnityEngine;
using UnityEditor;

public class CreateMapObjects
{
    [MenuItem("Tools/Moonlit Garden/Create Bridge and Shop")]
    public static void CreateObjects()
    {
        // Create Bridge
        CreateBridge();
        
        // Create Shop Building
        CreateShopBuilding();
        
        AssetDatabase.SaveAssets();
        Debug.Log("Bridge and Shop created!");
    }
    
    private static void CreateBridge()
    {
        // Load bridge sprite
        Sprite bridgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Tilemap/Bridge_Wooden.png");
        if (bridgeSprite == null)
        {
            Debug.LogError("Bridge_Wooden.png not found!");
            return;
        }
        
        // Create Bridge GameObject
        GameObject bridge = new GameObject("Bridge_Wooden", typeof(SpriteRenderer));
        bridge.transform.position = new Vector3(8, 1, 0); // Position over river
        
        SpriteRenderer sr = bridge.GetComponent<SpriteRenderer>();
        sr.sprite = bridgeSprite;
        sr.sortingLayerName = "Ground";
        sr.sortingOrder = 1;
        
        // Add collider for walking
        BoxCollider2D collider = bridge.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(2, 0.5f);
        collider.offset = new Vector2(0, 0);
        
        Debug.Log("Bridge created at (8, 1)");
    }
    
    private static void CreateShopBuilding()
    {
        // Load shop sprite
        Sprite shopSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ShopBuilding.png");
        if (shopSprite == null)
        {
            Debug.LogError("ShopBuilding.png not found!");
            return;
        }
        
        // Find ShopNPC position
        GameObject shopNPC = GameObject.Find("ShopNPC");
        Vector3 shopPos = new Vector3(12, 8, 0); // Default position
        
        if (shopNPC != null)
        {
            shopPos = shopNPC.transform.position + new Vector3(2, 0, 0); // Right of NPC
        }
        
        // Create Shop Building
        GameObject shop = new GameObject("ShopBuilding", typeof(SpriteRenderer));
        shop.transform.position = shopPos;
        
        SpriteRenderer sr = shop.GetComponent<SpriteRenderer>();
        sr.sprite = shopSprite;
        sr.sortingLayerName = "Buildings";
        sr.sortingOrder = 0;
        
        // Add collider
        BoxCollider2D collider = shop.AddComponent<BoxCollider2D>();
        
        Debug.Log($"Shop Building created at {shopPos}");
    }
}
