using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

/// <summary>
/// Mouse-only player controller: left-click-to-move + 8-direction cursor facing.
/// Movement is restricted to Tilemap_Path tiles (and the bridge walkable zone).
/// Water tiles are blocked per-frame to prevent the player from crossing rivers.
///
/// Approach: directly calls animator.CrossFade(stateName) by computing the state
/// name from direction — bypasses unreliable transition conditions in the controller.
/// </summary>
public class PlayerMovement_Mouse : MonoBehaviour
{
    public Rigidbody2D rb;
    public float speed = 5f;
    public Animator animator;

    [Header("Movement Bounds")]
    [Tooltip("Path tilemap — always walkable")]
    public Tilemap pathTilemap;

    [Tooltip("Grass tilemap — also walkable")]
    public Tilemap grassTilemap;

    [Tooltip("Water tilemap — player cannot move onto these tiles")]
    public Tilemap waterTilemap;


    // Stop moving when within this distance of the target
    private const float STOP_THRESHOLD = 0.15f;

    // How close the mouse must be to the player to register a direction change
    private const float MIN_FACING_DIST_SQ = 0.05f * 0.05f;

    // Blend time for crossfade between directional states (seconds)
    private const float BLEND_TIME = 0.05f;

    private Vector2 _targetPosition;
    private bool    _isMoving = false;
    private bool    _targetIsBridge = false; // true when destination is on a bridge
    private string  _currentStateName = "";

    // Stuck detection: stop walking if player hasn't moved for several frames
    private Vector2 _lastPosition;
    private int     _stuckFrames = 0;
    private const int STUCK_FRAME_LIMIT = 5;

    // Maps snapped 8-direction index → direction string matching Animator state suffix
    // Index: 0=E, 1=NE, 2=N, 3=NW, 4=W, 5=SW, 6=S, 7=SE
    private static readonly string[] DirectionNames = new string[]
    {
        "east", "north-east", "north", "north-west",
        "west", "south-west", "south", "south-east",
    };


    void Start()
    {
        // Auto-find grass tilemap if not assigned
        if (grassTilemap == null)
        {
            var grassGO = GameObject.Find("Tilemap_Grass");
            if (grassGO != null) grassTilemap = grassGO.GetComponent<Tilemap>();
        }
    }

    void Update()
    {
        // --- 1. Left-click sets move target (only on walkable path tiles) ---
        // Skip movement when clicking on UI elements (shop panel, inventory, etc.)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            bool walkable = IsWalkable(worldPos);
            bool bridgeAt = IsBridgeAtPoint(worldPos);
            Debug.LogWarning($"[Click] pos=({worldPos.x:F1},{worldPos.y:F1}) walkable={walkable} bridge={bridgeAt}");
            if (walkable)
            {
                _targetPosition = worldPos;
                _isMoving = true;
                _targetIsBridge = bridgeAt;
            }
        }
        if (Input.GetMouseButton(0) && _isMoving)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (IsWalkable(worldPos))
            {
                _targetPosition = worldPos;
            }
        }

        // Stop when close enough to target
        if (_isMoving && Vector2.Distance(rb.position, _targetPosition) <= STOP_THRESHOLD)
            _isMoving = false;

        // --- 2. Facing direction ---
        // While moving: face the walk direction (toward target)
        // While idle:   face the mouse cursor
        Vector2 facingVec;
        if (_isMoving)
        {
            facingVec = _targetPosition - rb.position;
        }
        else
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            facingVec = mouseWorld - rb.position;
        }

        string dirName = facingVec.sqrMagnitude > MIN_FACING_DIST_SQ
            ? DirectionNames[SnapIndex(facingVec)]
            : ExtractDirectionFromState(_currentStateName);

        // --- 3. Keep Speed parameter in sync so transition conditions don't fight CrossFade ---
        animator.SetFloat("Speed", _isMoving ? 1f : 0f);

        // --- 4. CrossFade only when state changes ---
        string action   = _isMoving ? "Walk" : "Idle";
        string newState = action + "_" + dirName;

        if (newState != _currentStateName)
        {
            animator.CrossFade(newState, BLEND_TIME, 0, 0f);
            _currentStateName = newState;
        }
    }

    void FixedUpdate()
    {
        if (!_isMoving)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Stuck detection: if player hasn't moved significantly, stop
        float movedDist = Vector2.Distance(rb.position, _lastPosition);
        if (movedDist < 0.01f)
        {
            _stuckFrames++;
            if (_stuckFrames >= STUCK_FRAME_LIMIT)
            {
                rb.linearVelocity = Vector2.zero;
                _isMoving = false;
                _stuckFrames = 0;
                return;
            }
        }
        else
        {
            _stuckFrames = 0;
        }
        _lastPosition = rb.position;

        Vector2 dir = (_targetPosition - rb.position).normalized;
        Vector2 nextPos = rb.position + dir * speed * Time.fixedDeltaTime;

        // Block movement into water-only tiles (no path tile on top, not on bridge)
        // Allow water crossing when the destination is a bridge (path→bridge transition)
        bool nextOnGround = IsOnGround(nextPos);
        bool nextOnWater = IsOnWater(nextPos);
        bool onBridge = IsPlayerOnBridge();

        if (nextOnWater && !nextOnGround && !onBridge && !_targetIsBridge)
        {
            rb.linearVelocity = Vector2.zero;
            _isMoving = false;
            return;
        }

        rb.linearVelocity = dir * speed;
    }

    // Reusable buffer for OverlapPoint results
    private static readonly Collider2D[] _overlapBuffer = new Collider2D[8];
    private static readonly ContactFilter2D _triggerFilter = new ContactFilter2D
    {
        useTriggers   = true,
        useLayerMask  = false,
        useDepth      = false,
        useOutsideDepth     = false,
        useNormalAngle      = false,
        useOutsideNormalAngle = false,
    };

    /// <summary>
    /// Returns true if the world position is walkable:
    ///   1. Path tile → always walkable (path is intentionally placed, even near water).
    ///   2. Water tile → only walkable via Bridge collider.
    ///   3. Grass tile (no water) → walkable.
    ///   4. Bridge collider → walkable.
    /// </summary>
    private bool IsWalkable(Vector3 worldPos)
    {
        // 1. Path tiles always win — they are intentionally placed walkable surfaces
        Vector3Int pathCell = pathTilemap != null ? pathTilemap.WorldToCell(worldPos) : Vector3Int.zero;
        if (pathTilemap != null && pathTilemap.HasTile(pathCell)) return true;

        // 2. If water tile exists, only bridge overrides it
        bool isWater = IsOnWater(worldPos);
        if (isWater)
        {
            Vector2 point = new Vector2(worldPos.x, worldPos.y);
            int count = Physics2D.OverlapPoint(point, _triggerFilter, _overlapBuffer);
            for (int i = 0; i < count; i++)
            {
                if (_overlapBuffer[i].CompareTag("Bridge"))
                    return true;
            }
            return false;
        }

        // 3. Grass tile (no water underneath) → walkable
        if (grassTilemap != null && grassTilemap.HasTile(grassTilemap.WorldToCell(worldPos))) return true;

        // 4. Bridge over non-water area
        {
            Vector2 point = new Vector2(worldPos.x, worldPos.y);
            int count = Physics2D.OverlapPoint(point, _triggerFilter, _overlapBuffer);
            for (int i = 0; i < count; i++)
            {
                if (_overlapBuffer[i].CompareTag("Bridge"))
                    return true;
            }
        }

        // 5. No ground, no bridge → blocked
        return false;
    }

    /// <summary>
    /// Returns true if the world position overlaps a "Bridge" trigger collider.
    /// Used to determine if the movement destination is on a bridge.
    /// </summary>
    private bool IsBridgeAtPoint(Vector3 worldPos)
    {
        Vector2 point = new Vector2(worldPos.x, worldPos.y);
        int count = Physics2D.OverlapPoint(point, _triggerFilter, _overlapBuffer);
        for (int i = 0; i < count; i++)
        {
            if (_overlapBuffer[i].CompareTag("Bridge"))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if the world position overlaps a path or grass tile (walkable ground).
    /// </summary>
    private bool IsOnGround(Vector2 worldPos)
    {
        Vector3 wp = new Vector3(worldPos.x, worldPos.y, 0);
        if (pathTilemap != null && pathTilemap.HasTile(pathTilemap.WorldToCell(wp))) return true;
        if (grassTilemap != null && grassTilemap.HasTile(grassTilemap.WorldToCell(wp))) return true;
        return false;
    }

    /// <summary>
    /// Returns true if the world position overlaps a water tile.
    /// </summary>
    private bool IsOnWater(Vector2 worldPos)
    {
        if (waterTilemap == null) return false;
        Vector3Int cell = waterTilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));
        return waterTilemap.HasTile(cell);
    }

    /// <summary>
    /// Returns true if the player is currently overlapping a bridge trigger.
    /// </summary>
    private bool IsPlayerOnBridge()
    {
        var detector = GetComponent<BridgeDetector>();
        return detector != null && detector.IsOnBridge;
    }

    /// <summary>
    /// Snaps direction vector to nearest of 8 directions (45° steps).
    /// Returns index into DirectionNames / Directions8 arrays.
    /// </summary>
    private static int SnapIndex(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;
        return Mathf.RoundToInt(angle / 45f) % 8;
    }

    /// <summary>
    /// Extracts direction suffix from a state name like "Idle_north-east" → "north-east".
    /// Falls back to "south" if state name is empty or malformed.
    /// </summary>
    private static string ExtractDirectionFromState(string stateName)
    {
        if (string.IsNullOrEmpty(stateName)) return "south";
        int underscore = stateName.IndexOf('_');
        return underscore >= 0 ? stateName.Substring(underscore + 1) : "south";
    }

    /// <summary>
    /// Cancels the current move target (e.g. when blocked by water).
    /// </summary>
    public void CancelMovement()
    {
        _isMoving = false;
        _targetIsBridge = false;
    }

    /// <summary>
    /// Public API: trigger a farming action animation (called by PlayerFarmController).
    /// Preserves current facing direction.
    /// </summary>
    public void PlayAction(string actionTrigger)
    {
        string dir      = ExtractDirectionFromState(_currentStateName);
        string newState = actionTrigger + "_" + dir;
        animator.CrossFade(newState, BLEND_TIME, 0, 0f);
        _currentStateName = "Idle_" + dir; // Will resume Idle after action exits
    }
}
