using UnityEngine;

/// <summary>
/// Mouse-driven player controller: click-to-move + continuous cursor facing.
/// The character always faces toward the current mouse cursor position,
/// regardless of whether they are moving or standing still.
/// </summary>
public class PlayerMovement_Mouse : MonoBehaviour
{
    public Rigidbody2D rb;
    public float speed = 5f;

    private Vector2 targetPosition;
    private Vector2 movement;
    private bool isMoving = false;

    public Animator animator;

    void Update()
    {
        // Click to set movement target
        if (Input.GetMouseButton(0))
        {
            targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isMoving = true;
        }

        // Always face toward current mouse cursor position
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 facingDir = (mouseWorld - rb.position);

        // Only update facing if cursor is far enough from player to determine direction
        if (facingDir.sqrMagnitude > 0.01f)
        {
            facingDir.Normalize();
            animator.SetFloat("Horizontal", facingDir.x);
            animator.SetFloat("Vertical", facingDir.y);
        }

        // Movement calculation
        if (isMoving)
        {
            movement = (targetPosition - rb.position).normalized;
            animator.SetFloat("Speed", 1f);
        }
        else
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);

            if (Vector2.Distance(rb.position, targetPosition) <= 0.1f)
            {
                isMoving = false;
            }
        }
    }
}
