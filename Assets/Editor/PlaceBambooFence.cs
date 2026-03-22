using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

/// <summary>
/// Editor tool: reads Tilemap_Path bounds and places bamboo fence sprites around its perimeter.
/// Run via menu: Tools > Moonlit Garden > Place Bamboo Fence Around Path
/// </summary>
public class PlaceBambooFence : EditorWindow
{
    private const string FENCE_PARENT_NAME = "FenceBamboo";
    private const string TILEMAP_PATH_NAME = "Tilemap_Path";

    private const string SPRITE_H  = "Assets/Sprites/Fences/fence_bamboo_horizontal.png";
    private const string SPRITE_V  = "Assets/Sprites/Fences/fence_bamboo_vertical.png";
    private const string SPRITE_C  = "Assets/Sprites/Fences/fence_bamboo_corner.png";

    // Fence sprite world-size in Unity units (assuming 32 PPU for 32px sprites, 64px wide = 2 units)
    private const float TILE_SIZE  = 1f;   // 1 Unity unit per tile cell
    private const int   PIXELS_PER_UNIT = 32;

    [MenuItem("Tools/Moonlit Garden/Place Bamboo Fence Around Path")]
    public static void PlaceFence()
    {
        // --- 1. Find Tilemap_Path ---
        var tilemapGO = GameObject.Find(TILEMAP_PATH_NAME);
        if (tilemapGO == null)
        {
            Debug.LogError("[BambooFence] Cannot find GameObject named: " + TILEMAP_PATH_NAME);
            return;
        }

        var tilemap = tilemapGO.GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("[BambooFence] Tilemap_Path has no Tilemap component.");
            return;
        }

        tilemap.CompressBounds();
        BoundsInt bounds = tilemap.cellBounds;
        Debug.Log($"[BambooFence] Tilemap bounds: {bounds}");

        // --- 2. Load sprites ---
        var spriteH = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_H);
        var spriteV = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_V);
        var spriteC = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_C);

        if (spriteH == null || spriteV == null || spriteC == null)
        {
            Debug.LogError("[BambooFence] Missing fence sprites. Check Assets/Sprites/Fences/");
            return;
        }

        // --- 3. Remove old fence if re-running ---
        var oldParent = GameObject.Find(FENCE_PARENT_NAME);
        if (oldParent != null)
        {
            Undo.DestroyObjectImmediate(oldParent);
        }

        // --- 4. Create parent container ---
        var parent = new GameObject(FENCE_PARENT_NAME);
        Undo.RegisterCreatedObjectUndo(parent, "Place Bamboo Fence");

        // Place parent near Tilemap_Path
        parent.transform.SetParent(tilemapGO.transform.parent, false);

        // --- 5. Compute world-space bounds ---
        // tilemap.CellToWorld returns bottom-left corner of each cell
        Vector3 worldMin = tilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMin, 0));
        Vector3 worldMax = tilemap.CellToWorld(new Vector3Int(bounds.xMax, bounds.yMax, 0));

        float left   = worldMin.x;
        float right  = worldMax.x;
        float bottom = worldMin.y;
        float top    = worldMax.y;

        float z = tilemapGO.transform.position.z - 0.1f; // render slightly in front

        // --- 6. Place fence pieces along each edge ---
        // BOTTOM & TOP edges (horizontal)
        for (int cx = bounds.xMin; cx < bounds.xMax; cx++)
        {
            float wx = tilemap.CellToWorld(new Vector3Int(cx, 0, 0)).x + TILE_SIZE * 0.5f;

            // Bottom edge
            CreateFenceSprite(parent, spriteH,
                new Vector3(wx, bottom, z), 0f, "Fence_H_Bot");

            // Top edge
            CreateFenceSprite(parent, spriteH,
                new Vector3(wx, top, z), 0f, "Fence_H_Top");
        }

        // LEFT & RIGHT edges (vertical)
        for (int cy = bounds.yMin; cy < bounds.yMax; cy++)
        {
            float wy = tilemap.CellToWorld(new Vector3Int(0, cy, 0)).y + TILE_SIZE * 0.5f;

            // Left edge
            CreateFenceSprite(parent, spriteV,
                new Vector3(left, wy, z), 0f, "Fence_V_Left");

            // Right edge
            CreateFenceSprite(parent, spriteV,
                new Vector3(right, wy, z), 0f, "Fence_V_Right");
        }

        // CORNERS (4 corners)
        CreateFenceSprite(parent, spriteC, new Vector3(left,  bottom, z), 0f,  "Fence_Corner_BL");
        CreateFenceSprite(parent, spriteC, new Vector3(right, bottom, z), 90f, "Fence_Corner_BR");
        CreateFenceSprite(parent, spriteC, new Vector3(left,  top,    z), 270f,"Fence_Corner_TL");
        CreateFenceSprite(parent, spriteC, new Vector3(right, top,    z), 180f,"Fence_Corner_TR");

        EditorUtility.SetDirty(parent);
        Debug.Log("[BambooFence] Done! FenceBamboo placed around Tilemap_Path.");
    }

    private static void CreateFenceSprite(GameObject parent, Sprite sprite,
        Vector3 worldPos, float rotationZ, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.position = worldPos;
        go.transform.rotation = Quaternion.Euler(0, 0, rotationZ);
        go.transform.localScale = Vector3.one;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 5;
    }
}
