using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Handles player movement with proper diagonal normalization for Vietnamese Farmer character.
/// Supports 8-directional movement with 7 actions (Idle, Walk, PickUp, Dig, Plant, Water, Harvest).
/// Phase 1 Fix (T5): Normalizes movement vector to prevent faster diagonal movement.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public float speed = 5f;

    private Vector2 movement;
    private Vector2 lastMovement;

    public Animator animator;

    [Header("Water Blocking")]
    [Tooltip("Water tilemap — keyboard movement is also blocked on water tiles")]
    public Tilemap waterTilemap;
    
    // Action triggers
    private bool triggerPickUp;
    private bool triggerDig;
    private bool triggerPlant;
    private bool triggerWater;
    private bool triggerHarvest;

    // Update is called once per frame
    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        
        // Phase 1 Fix (T5): Normalize diagonal movement
        // Without this, diagonal movement (e.g., W+D) would have magnitude sqrt(2) ≈ 1.414
        // causing the player to move ~41% faster diagonally
        if (movement.sqrMagnitude > 1)
        {
            movement.Normalize();
        }

        // Update animator parameters only when keyboard is active
        // (avoids overwriting PlayerMovement_Mouse animator state)
        bool hasKeyboardInput = movement.sqrMagnitude > 0.01f;
        if (animator != null && hasKeyboardInput)
        {
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetFloat("Speed", movement.sqrMagnitude);

            // Store last movement direction for action animations
            if (movement.sqrMagnitude > 0.1f)
            {
                lastMovement = movement;
            }
            
            // Handle action triggers
            if (triggerPickUp)
            {
                animator.SetTrigger("PickUp");
                triggerPickUp = false;
            }
            if (triggerDig)
            {
                animator.SetTrigger("Dig");
                triggerDig = false;
            }
            if (triggerPlant)
            {
                animator.SetTrigger("Plant");
                triggerPlant = false;
            }
            if (triggerWater)
            {
                animator.SetTrigger("Water");
                triggerWater = false;
            }
            if (triggerHarvest)
            {
                animator.SetTrigger("Harvest");
                triggerHarvest = false;
            }
        }
    }

    void FixedUpdate()
    {
        // Only move if keyboard has input (don't interfere with mouse movement)
        if (movement.sqrMagnitude > 0.01f)
        {
            Vector2 nextPos = rb.position + movement * speed * Time.fixedDeltaTime;

            // Block movement onto water tiles
            if (waterTilemap != null)
            {
                Vector3Int cell = waterTilemap.WorldToCell(new Vector3(nextPos.x, nextPos.y, 0));
                if (waterTilemap.HasTile(cell))
                    return; // Don't move onto water
            }

            rb.MovePosition(nextPos);
        }
    }
    
    /// <summary>
    /// Trigger the PickUp animation
    /// </summary>
    public void DoPickUp()
    {
        triggerPickUp = true;
    }
    
    /// <summary>
    /// Trigger the Dig animation
    /// </summary>
    public void DoDig()
    {
        triggerDig = true;
    }
    
    /// <summary>
    /// Trigger the Plant animation
    /// </summary>
    public void DoPlant()
    {
        triggerPlant = true;
    }
    
    /// <summary>
    /// Trigger the Water animation
    /// </summary>
    public void DoWater()
    {
        triggerWater = true;
    }
    
    /// <summary>
    /// Trigger the Harvest animation
    /// </summary>
    public void DoHarvest()
    {
        triggerHarvest = true;
    }
    
    /// <summary>
    /// Get the last movement direction (normalized)
    /// </summary>
    public Vector2 GetLastMovementDirection()
    {
        return lastMovement.normalized;
    }
}
