using UnityEngine;

public class BridgeInitializer : MonoBehaviour
{
    [SerializeField] private string spriteName = "Bridge_Straight";
    
    void Start()
    {
        // Load sprite from Resources
        Sprite sprite = Resources.Load<Sprite>($"BridgeTileset/{spriteName}");
        
        if (sprite == null)
        {
            // Try loading from Sprites folder
            sprite = Resources.Load<Sprite>(spriteName);
        }
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sprite != null)
        {
            sr.sprite = sprite;
        }
        else if (sr != null && sprite == null)
        {
            Debug.LogWarning("Bridge sprite not found in Resources. Please assign manually in Inspector.");
        }
        
        // Configure collider
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = new Vector2(0.9f, 0.3f);
            col.offset = new Vector2(0, 0);
        }
    }
}
