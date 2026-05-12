using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 10f;

    [Header("Capsule - assumes transform.position is at character center")]
    public float characterRadius = 0.25f;
    // Total height of the character
    public float characterHeight = 2f;

    [Header("Slope & Steps")]
    public float maxSlopeAngle = 45f;
    public float stepHeight = 0.3f;
    public float stepCooldown = 0.1f;

    [Header("Gravity")]
    public float gravity = -19.8f;
    // Extra distance below feet the ground check reaches.
    // Increase if player floats; decrease if they stick to ceilings.
    public float groundCheckTolerance = 0.15f;

    [Header("Collision")]
    // Small buffer added to casts to catch geometry before overlap occurs
    public float skinWidth = 0.05f;

    private InputAction moveAction;
    private float verticalVelocity;
    private Vector3 moveDir;
    private float moveDistance;
    public bool isWalking;

    private Vector3 knockbackVelocity;
    private float knockbackTimer;
    public float knockbackDecay = 10f;
    private float stunTimer;
    private float lastStepTime;

    [Header("Animation")]
    private Animator animator;
    
    // Derived from characterHeight — feet and half height for readability
    private float HalfHeight => characterHeight * 0.5f;

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        animator = GetComponentInChildren<Animator>();
    }

    void FixedUpdate()
    {
        GetInput();
        HandleHorizontalMovement();
        HandleKnockback();
        HandleGravity();
        RotatePlayer();
        animator.SetBool("IsWalking", isWalking);

        if (stunTimer > 0) stunTimer -= Time.fixedDeltaTime;

        Debug.DrawRay(transform.position, transform.forward * 2f, Color.violet);
    }

    // =====================
    // INPUT
    // =====================
    private void GetInput()
    {
        Vector2 inputVector = moveAction.ReadValue<Vector2>();
        moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        moveDistance = moveSpeed * Time.fixedDeltaTime;
        isWalking = moveDir != Vector3.zero;
    }

    // =====================
    // HORIZONTAL MOVEMENT
    // =====================
    private void HandleHorizontalMovement()
    {
        if (stunTimer > 0 || knockbackTimer > 0) return;
        if (moveDir == Vector3.zero) return;

        if (TryMove(moveDir)) return;
        if (TryStep(moveDir)) return;
        TrySlide(moveDir);
    }

    private bool TryMove(Vector3 dir)
    {
        if (IsBlocked(dir, moveDistance)) return false;
        MoveOnSurface(dir, moveDistance);
        return true;
    }

    private bool TryStep(Vector3 dir)
    {
        if (!IsGrounded(out _)) return false;
        if (Time.time - lastStepTime < stepCooldown) return false;

        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, moveDistance + skinWidth))
        {
            if (Vector3.Angle(hit.normal, Vector3.up) <= maxSlopeAngle) return false;
        }

        // Check if we can move forward after stepping up
        Vector3 stepOffset = Vector3.up * stepHeight;
        Vector3 raisedBottom = CapsuleBottom() + stepOffset;
        Vector3 raisedTop    = CapsuleTop()    + stepOffset;

        bool canStep = !Physics.CapsuleCast(raisedBottom, raisedTop, characterRadius, dir, moveDistance);
        if (!canStep) return false;

        transform.position += stepOffset;
        MoveOnSurface(dir, moveDistance);
        lastStepTime = Time.time;
        return true;
    }

    private void TrySlide(Vector3 dir)
    {
        Vector3 dirX = new Vector3(dir.x, 0, 0).normalized;
        Vector3 dirZ = new Vector3(0, 0, dir.z).normalized;

        if (dir.x != 0 && !IsBlocked(dirX, moveDistance))
        {
            // How aligned is the intended direction with the slide direction.
            // Close to 1 = nearly parallel to wall, close to 0 = nearly head-on.
            float alignment = Mathf.Abs(Vector3.Dot(dir, dirX));
            MoveOnSurface(dirX, moveDistance * alignment);
            return;
        }

        if (dir.z != 0 && !IsBlocked(dirZ, moveDistance))
        {
            float alignment = Mathf.Abs(Vector3.Dot(dir, dirZ));
            MoveOnSurface(dirZ, moveDistance * alignment);
        }
    }

    private void MoveOnSurface(Vector3 dir, float distance)
    {
        Vector3 move;

        if (IsGrounded(out RaycastHit hit))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (slopeAngle <= maxSlopeAngle)
            {
                // Project onto slope to get the right XZ ratio for inclines,
                // then strip Y entirely — gravity owns vertical positioning
                Vector3 slopeDir = Vector3.ProjectOnPlane(dir, hit.normal).normalized;
                move = new Vector3(slopeDir.x, 0, slopeDir.z) * distance;
                transform.position += move;
                return;
            }
        }

        // Not grounded or too steep — move flat
        move = new Vector3(dir.x, 0, dir.z) * distance;
        transform.position += move;
    }

    // =====================
    // COLLISION
    // =====================

    // Capsule cast points derived from center-pivot position
    private Vector3 CapsuleBottom() => transform.position - Vector3.up * (HalfHeight - characterRadius);
    private Vector3 CapsuleTop()    => transform.position + Vector3.up * (HalfHeight - characterRadius);

    private bool IsBlocked(Vector3 dir, float distance)
    {
        return Physics.CapsuleCast(
            CapsuleBottom(), CapsuleTop(),
            characterRadius, dir,
            distance + skinWidth
        );
    }

    // =====================
    // GRAVITY
    // =====================
    private void HandleGravity()
    {
        if (IsGrounded(out RaycastHit hit))
        {
            verticalVelocity = 0f;
            // Snap feet to ground: ground point + half height brings center up correctly
            float groundY = hit.point.y + HalfHeight;
            transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
        }
        else
        {
            verticalVelocity += gravity * Time.fixedDeltaTime;
            transform.position += Vector3.up * (verticalVelocity * Time.fixedDeltaTime);
        }
    }

    private bool IsGrounded(out RaycastHit hit)
    {
        // Ray starts at center, reaches down past feet by groundCheckTolerance
        return Physics.Raycast(
            transform.position,
            Vector3.down,
            out hit,
            HalfHeight + groundCheckTolerance
        );
    }

    // =====================
    // ROTATION
    // =====================
    private void RotatePlayer()
    {
        
        if (moveDir == Vector3.zero) return;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.fixedDeltaTime * rotateSpeed);
    }

    // =====================
    // KNOCKBACK
    // =====================
    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        knockbackVelocity = direction.normalized * force;
        knockbackTimer = duration;
    }

    private void HandleKnockback()
    {
        if (knockbackTimer <= 0) return;

        Vector3 dir = knockbackVelocity.normalized;
        float distance = knockbackVelocity.magnitude * Time.fixedDeltaTime;

        if (!IsBlocked(dir, distance))
            MoveOnSurface(dir, distance);

        knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDecay * Time.fixedDeltaTime);
        knockbackTimer -= Time.fixedDeltaTime;
    }

    public void ApplyStun(float duration)
    {
        stunTimer = duration;
    }

    // =====================
    // GIZMOS
    // =====================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(CapsuleBottom(), characterRadius);
        Gizmos.DrawWireSphere(CapsuleTop(), characterRadius);
        Gizmos.DrawLine(
            CapsuleBottom() + Vector3.forward * characterRadius,
            CapsuleTop()    + Vector3.forward * characterRadius
        );
        Gizmos.DrawLine(
            CapsuleBottom() - Vector3.forward * characterRadius,
            CapsuleTop()    - Vector3.forward * characterRadius
        );
    }
}