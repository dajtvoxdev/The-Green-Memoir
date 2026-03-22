using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Prevents the player from walking on water tiles.
///
/// Approach: Instead of relying on OnTriggerEnter2D (unreliable with TilemapCollider2D),
/// directly checks each FixedUpdate whether the player's position overlaps a water tile.
/// If so, snaps the player back to the last safe (non-water) position.
///
/// Setup: Attach to the Tilemap_Water GameObject (must have a Tilemap component).
/// </summary>
public class WaterCollision : MonoBehaviour
{
    private Tilemap waterTilemap;
    private Tilemap pathTilemap;
    private Tilemap grassTilemap;
    private Rigidbody2D playerRigidbody;
    private PlayerMovement_Mouse mouseController;
    private Vector2 lastSafePosition;
    private int bridgeOverlapCount = 0;

    private bool IsOnBridge => bridgeOverlapCount > 0;

    private void Awake()
    {
        waterTilemap = GetComponent<Tilemap>();
        if (waterTilemap == null)
        {
            Debug.LogError("[WaterCollision] No Tilemap component found on " + gameObject.name);
            return;
        }

        var pathGO = GameObject.Find("Tilemap_Path");
        if (pathGO != null) pathTilemap = pathGO.GetComponent<Tilemap>();
        var grassGO = GameObject.Find("Tilemap_Grass");
        if (grassGO != null) grassTilemap = grassGO.GetComponent<Tilemap>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerRigidbody = player.GetComponent<Rigidbody2D>();
            mouseController = player.GetComponent<PlayerMovement_Mouse>();
            lastSafePosition = player.transform.position;

            // Attach bridge detection to the player
            var bridgeDetector = player.GetComponent<BridgeDetector>();
            if (bridgeDetector == null)
            {
                bridgeDetector = player.AddComponent<BridgeDetector>();
            }
            bridgeDetector.waterCollision = this;
        }
        else
        {
            Debug.LogError("[WaterCollision] No Player found with tag 'Player'");
        }
    }

    private void FixedUpdate()
    {
        if (playerRigidbody == null || waterTilemap == null) return;

        Vector2 playerPos = playerRigidbody.position;
        bool isOnWaterTile = IsPositionOnWater(playerPos);
        bool isOnGroundTile = IsPositionOnGround(playerPos);

        // Only block if on water AND not on ground AND not on bridge
        if (isOnWaterTile && !isOnGroundTile && !IsOnBridge)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.MovePosition(lastSafePosition);
            if (mouseController != null) mouseController.CancelMovement();
        }
        else
        {
            lastSafePosition = playerPos;
        }
    }

    /// <summary>
    /// Checks if a world position falls on a walkable ground tile (path or grass).
    /// </summary>
    private bool IsPositionOnGround(Vector2 worldPos)
    {
        Vector3 wp = new Vector3(worldPos.x, worldPos.y, 0);
        if (pathTilemap != null && pathTilemap.HasTile(pathTilemap.WorldToCell(wp))) return true;
        if (grassTilemap != null && grassTilemap.HasTile(grassTilemap.WorldToCell(wp))) return true;
        return false;
    }

    /// <summary>
    /// Checks if a world position falls on a water tile.
    /// </summary>
    private bool IsPositionOnWater(Vector2 worldPos)
    {
        Vector3Int cell = waterTilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));
        return waterTilemap.HasTile(cell);
    }

    /// <summary>Called by BridgeDetector when player enters a bridge trigger.</summary>
    public void OnBridgeEnter()
    {
        bridgeOverlapCount++;
    }

    /// <summary>Called by BridgeDetector when player exits a bridge trigger.</summary>
    public void OnBridgeExit()
    {
        bridgeOverlapCount = Mathf.Max(0, bridgeOverlapCount - 1);
    }
}

/// <summary>
/// Detects when the player overlaps a bridge collider (tagged "Bridge").
/// Attached automatically by WaterCollision — no manual setup needed.
/// Exposes IsOnBridge for use by both WaterCollision and PlayerMovement_Mouse.
/// </summary>
public class BridgeDetector : MonoBehaviour
{
    [HideInInspector]
    public WaterCollision waterCollision;

    private int _overlapCount;

    /// <summary>True when the player is overlapping at least one bridge trigger.</summary>
    public bool IsOnBridge => _overlapCount > 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bridge"))
        {
            _overlapCount++;
            waterCollision?.OnBridgeEnter();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bridge"))
        {
            _overlapCount = Mathf.Max(0, _overlapCount - 1);
            waterCollision?.OnBridgeExit();
        }
    }
}
