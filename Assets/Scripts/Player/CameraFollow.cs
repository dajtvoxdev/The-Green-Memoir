using UnityEngine;

/// <summary>
/// 2D Camera follow system for the player.
/// Phase 1 Feature (#12): Smooth camera follow with boundary clamping.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The target transform to follow (usually the player)")]
    public Transform target;
    
    [Header("Follow Settings")]
    [Tooltip("How smoothly the camera follows the target (higher = snappier)")]
    public float smoothSpeed = 5f;
    
    [Tooltip("Offset from the target position")]
    public Vector3 offset = Vector3.zero;
    
    [Header("Boundaries")]
    [Tooltip("Whether to clamp camera to bounds")]
    public bool useBounds = true;
    
    [Tooltip("Minimum X position for the camera")]
    public float minX = -100f;
    
    [Tooltip("Maximum X position for the camera")]
    public float maxX = 100f;
    
    [Tooltip("Minimum Y position for the camera")]
    public float minY = -100f;
    
    [Tooltip("Maximum Y position for the camera")]
    public float maxY = 100f;
    
    [Header("Zoom")]
    [Tooltip("Whether to enable zoom functionality")]
    public bool enableZoom = false;
    
    [Tooltip("Minimum camera orthographic size")]
    public float minZoom = 5f;
    
    [Tooltip("Maximum camera orthographic size")]
    public float maxZoom = 15f;
    
    [Tooltip("Current zoom level")]
    public float currentZoom = 10f;
    
    private Camera mainCamera;
    private Vector3 velocity = Vector3.zero;
    
    void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("CameraFollow: No Camera component found on this GameObject!");
        }
    }
    
    void Start()
    {
        if (target == null)
        {
            // Try to find player by tag
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("CameraFollow: Found player by tag");
            }
            else
            {
                Debug.LogWarning("CameraFollow: No target set and no Player found!");
            }
        }
        
        // Initialize camera size
        if (mainCamera != null && enableZoom)
        {
            mainCamera.orthographicSize = currentZoom;
        }
    }
    
    /// <summary>
    /// LateUpdate is called after all Update functions.
    /// This ensures the camera follows the player after the player has moved.
    /// </summary>
    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }
        
        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;
        
        // Clamp to bounds if enabled
        if (useBounds)
        {
            desiredPosition = ClampToBounds(desiredPosition);
        }
        
        // Smoothly interpolate to the desired position
        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position, 
            desiredPosition, 
            ref velocity, 
            1f / smoothSpeed
        );
        
        transform.position = smoothedPosition;
        
        // Handle zoom input
        if (enableZoom)
        {
            HandleZoom();
        }
    }
    
    /// <summary>
    /// Clamps the camera position within the defined bounds.
    /// </summary>
    /// <param name="pos">The position to clamp</param>
    /// <returns>The clamped position</returns>
    private Vector3 ClampToBounds(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        // Keep z position unchanged for 2D
        pos.z = transform.position.z;
        return pos;
    }
    
    /// <summary>
    /// Handles mouse wheel zoom input.
    /// </summary>
    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            currentZoom -= scrollInput * 5f;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            
            if (mainCamera != null)
            {
                mainCamera.orthographicSize = currentZoom;
            }
        }
    }
    
    /// <summary>
    /// Sets the bounds for the camera.
    /// </summary>
    /// <param name="min">Minimum bounds</param>
    /// <param name="max">Maximum bounds</param>
    public void SetBounds(Vector2 min, Vector2 max)
    {
        minX = min.x;
        minY = min.y;
        maxX = max.x;
        maxY = max.y;
        useBounds = true;
    }
    
    /// <summary>
    /// Sets the target to follow.
    /// </summary>
    /// <param name="newTarget">The new target transform</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    /// <summary>
    /// Instantly snaps the camera to the target position (no smoothing).
    /// Useful for scene transitions.
    /// </summary>
    public void SnapToTarget()
    {
        if (target != null)
        {
            Vector3 pos = target.position + offset;
            if (useBounds)
            {
                pos = ClampToBounds(pos);
            }
            transform.position = pos;
        }
    }
}