using UnityEngine;

[ExecuteInEditMode]
public class BridgeAutoSetup : MonoBehaviour
{
    [SerializeField] private string spritePath = "Assets/Sprites/BridgeTileset/Bridge_Straight.png";
    
    void Awake()
    {
        SetupBridge();
    }
    
    void SetupBridge()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;
        
        // Load sprite from resources or use default
        Sprite sprite = Resources.Load<Sprite>("BridgeTileset/Bridge_Straight");
        if (sprite != null)
        {
            sr.sprite = sprite;
        }
        
        // Configure renderer
        sr.sortingLayerName = "Objects";
        sr.sortingOrder = 10;
        
        // Scale to make a long bridge (5 tiles wide)
        transform.localScale = new Vector3(5f, 1.5f, 1f);
        
        // Configure collider
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = new Vector2(0.9f, 0.3f);
            col.offset = new Vector2(0, 0);
        }
        
        // Destroy this script after setup
        if (Application.isPlaying)
        {
            Destroy(this);
        }
    }
}
