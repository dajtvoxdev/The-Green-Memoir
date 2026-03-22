using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Shows a visual highlight on the tilemap tile currently targeted by the player.
/// Snaps to the grid cell at the player's feet position.
///
/// Phase 3 Feature (#2 improvement): Tile selection cursor.
///
/// Usage: Attach to a child GameObject of the player (or a separate GO).
/// Assign groundTilemap and optionally a custom cursorSprite.
/// Color changes: yellow = empty/grass, blue = watered crop, green = harvestable.
/// </summary>
public class TileCursor : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The ground tilemap — used to convert world pos to cell center.")]
    public Tilemap groundTilemap;

    [Tooltip("The player transform to follow.")]
    public Transform playerTransform;

    [Header("Cursor Appearance")]
    [Tooltip("Optional custom cursor sprite. If null, uses a generated square.")]
    public Sprite cursorSprite;

    [Tooltip("Cursor tint color (default: semi-transparent yellow).")]
    public Color cursorColor = new Color(1f, 0.95f, 0.3f, 0.5f);

    [Tooltip("Color when tile is already watered today.")]
    public Color wateredColor = new Color(0.3f, 0.7f, 1f, 0.4f);

    [Tooltip("Color when tile has a harvestable crop.")]
    public Color harvestColor = new Color(0.2f, 1f, 0.3f, 0.55f);

    [Header("Size")]
    [Tooltip("Manual scale multiplier for the cursor. Adjust in Inspector until it fits.")]
    [Range(0.05f, 2f)]
    public float cursorScale = 0.5f;

    [Header("Animation")]
    [Tooltip("Pulse animation speed.")]
    public float pulseSpeed = 2f;

    [Tooltip("Pulse scale range added to base 1.0 scale.")]
    public float pulseAmount = 0.06f;

    private SpriteRenderer _sr;
    private TileMapManager _tileMapManager;
    private Vector3Int _lastCell = new Vector3Int(int.MinValue, 0, 0);

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null)
        {
            _sr = gameObject.AddComponent<SpriteRenderer>();
        }

        _sr.sortingOrder = 15; // Above tilemap, below player
        _sr.sortingLayerName = "Default";

        _sr.sprite = cursorSprite != null ? cursorSprite : CreateBorderSprite();
        _sr.color = cursorColor;
    }

    void Start()
    {
        _tileMapManager = FindAnyObjectByType<TileMapManager>();

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (groundTilemap == null)
        {
            // Auto-detect by name
            Tilemap[] tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            foreach (var tm in tilemaps)
            {
                if (tm.name.Contains("Ground") || tm.name.Contains("Grass"))
                {
                    groundTilemap = tm;
                    break;
                }
            }
        }
    }

    void LateUpdate()
    {
        if (groundTilemap == null || Camera.main == null)
        {
            _sr.enabled = false;
            return;
        }

        // Follow mouse position for right-click farming feedback
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cell = groundTilemap.WorldToCell(mouseWorld);

        // Snap cursor to cell center
        Vector3 cellCenter = groundTilemap.GetCellCenterWorld(cell);
        transform.position = new Vector3(cellCenter.x, cellCenter.y, 0f);

        // Hide if too far from player (beyond interact range)
        if (playerTransform != null)
        {
            float dist = Vector2.Distance(playerTransform.position, cellCenter);
            if (dist > 3f)
            {
                _sr.enabled = false;
                return;
            }
        }

        // Color based on tile state
        UpdateCursorColor(cell);

        // Scale cursor with manual cursorScale, compensating for parent scale
        Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        float sx = cursorScale / Mathf.Max(parentScale.x, 0.01f);
        float sy = cursorScale / Mathf.Max(parentScale.y, 0.01f);
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = new Vector3(sx * pulse, sy * pulse, 1f);

        _sr.enabled = true;
        _lastCell = cell;
    }

    /// <summary>
    /// Changes cursor color to reflect the tile's current crop state.
    /// Green = harvestable, Blue = watered today, Yellow = default.
    /// </summary>
    private void UpdateCursorColor(Vector3Int cell)
    {
        if (_tileMapManager == null)
        {
            _sr.color = cursorColor;
            return;
        }

        TilemapDetail tile = _tileMapManager.GetTileAt(cell.x, cell.y);

        if (tile != null && tile.HasCrop)
        {
            if (tile.growthStage == (int)GrowthStage.Harvestable)
            {
                _sr.color = harvestColor;
            }
            else if (IsWateredToday(tile.lastWateredAt))
            {
                _sr.color = wateredColor;
            }
            else
            {
                _sr.color = cursorColor;
            }
        }
        else
        {
            _sr.color = cursorColor;
        }
    }

    /// <summary>
    /// Returns the current targeted cell (readable by other systems).
    /// </summary>
    public Vector3Int GetTargetCell() => _lastCell;

    /// <summary>
    /// Creates a border-only square sprite (thin outline, not filled) at runtime.
    /// Matches tilemap cell size (32px at PPU 32 = 1 world unit).
    /// </summary>
    private Sprite CreateBorderSprite()
    {
        int size = 32;
        int border = 2; // 2px border for clean pixel look
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        for (int px = 0; px < size; px++)
        {
            for (int py = 0; py < size; py++)
            {
                bool isBorder = px < border || px >= size - border
                             || py < border || py >= size - border;
                tex.SetPixel(px, py, isBorder ? Color.white : Color.clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }

    /// <summary>
    /// Checks if a unix-ms timestamp falls within today (UTC).
    /// </summary>
    private static bool IsWateredToday(long unixMs)
    {
        if (unixMs <= 0) return false;
        var wateredDate = System.DateTimeOffset.FromUnixTimeMilliseconds(unixMs).UtcDateTime.Date;
        return wateredDate == System.DateTime.UtcNow.Date;
    }
}
