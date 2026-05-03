using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public InputAction moveAction;

    public float moveSpeed = 5f;
    public float rotateSpeed = 10f;

    public float gravity = -19.8f;
    private float verticalVelocity;

    public float playerRadius = 0.25f;
    public float playerHeight = 0.5f;

    public float maxSlopeAngle = 45f;
    public float stepHeight = 0.3f;
    public float stepCooldown = 0.1f;
    private float lastStepTime;

    private Vector3 moveDir;
    private float moveDistance;

    public bool isWalking;
    
    private Vector3 knockbackVelocity;
    private float knockbackTimer;
    public float knockbackDecay = 10f;

    private float stunTimer;

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
    }

    void FixedUpdate()
    {
        GetInput();

        HandleHorizontalMovement();
        HandleGravity();

        RotatePlayer();
        
        HandleKnockback();

        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
        }
        
        Debug.DrawRay(transform.position, transform.forward * 2f, Color.violet);
        
    }

    
    private void GetInput()
    {
        Vector2 inputVector = moveAction.ReadValue<Vector2>();
        moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        //if (moveDir.magnitude > 1f) moveDir.Normalize();

        moveDistance = moveSpeed * Time.deltaTime;
        isWalking = moveDir != Vector3.zero;
    }

    
    private void HandleHorizontalMovement()
    {
        if (stunTimer > 0) return;
        
        if (moveDir == Vector3.zero) return;

        if (TryMove(moveDir)) return;

        if (TryStep(moveDir)) return;

        TrySlide(moveDir);
    }

    private bool TryMove(Vector3 dir)
    {
        if (!IsBlocked(dir))
        {
            MoveOnSurface(dir);
            return true;
        }
        return false;
    }

    private bool TryStep(Vector3 dir)
    {
        if (!IsGrounded(out _)) return false;
        if (Time.time - lastStepTime < stepCooldown) return false;

        
        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, moveDistance + 0.1f))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);

            if (angle <= maxSlopeAngle) return false; //  slope not a step
        }

        Vector3 stepUp = Vector3.up * stepHeight;

        bool canStep = !Physics.CapsuleCast(
            transform.position + stepUp,
            transform.position + stepUp + Vector3.up * playerHeight,
            playerRadius,
            dir,
            moveDistance
        );

        if (canStep)
        {
            transform.position += stepUp;
            MoveOnSurface(dir);

            lastStepTime = Time.time;
            return true;
        }

        return false;
    }

    private void TrySlide(Vector3 dir)
    {
        Vector3 dirX = new Vector3(dir.x, 0, 0).normalized;
        if (dir.x != 0 && !IsBlocked(dirX))
        {
            MoveOnSurface(dirX);
            return;
        }

        Vector3 dirZ = new Vector3(0, 0, dir.z).normalized;
        if (dir.z != 0 && !IsBlocked(dirZ))
        {
            MoveOnSurface(dirZ);
        }
    }

    private void MoveOnSurface(Vector3 dir)
    {
        if (IsGrounded(out RaycastHit hit))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (slopeAngle <= maxSlopeAngle)
            {
                Vector3 slopeDir = Vector3.ProjectOnPlane(dir, hit.normal).normalized;
                transform.position += slopeDir * moveDistance;
                return;
            }
        }

        transform.position += dir * moveDistance;
    }

    private bool IsBlocked(Vector3 dir)
    {
        return Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, dir, moveDistance);
    }

    
    private void HandleGravity()
    {
        if (IsGrounded(out RaycastHit hit))
        {
            verticalVelocity = 0f;

            // Snap to ground (prevents floating/jitter)
            float groundY = hit.point.y + playerHeight;
            transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
            transform.position += Vector3.up * (verticalVelocity * Time.deltaTime);
        }
    }

    
    private void RotatePlayer()
    {
        if (moveDir == Vector3.zero) return;

        transform.forward = Vector3.Slerp(
            transform.forward,
            moveDir,
            Time.deltaTime * rotateSpeed
        );
    }

    
    private bool IsGrounded(out RaycastHit hit)
    {
        return Physics.Raycast(transform.position, Vector3.down, out hit, playerHeight + 0.1f);
    }
    
    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        knockbackVelocity = direction.normalized * force;
        knockbackTimer = duration;
    }

    private void HandleKnockback()
    {
        if (knockbackTimer > 0)
        {
            transform.position += knockbackVelocity * Time.deltaTime;

            knockbackVelocity = Vector3.Lerp(
                knockbackVelocity,
                Vector3.zero,
                knockbackDecay * Time.deltaTime
            );

            knockbackTimer -= Time.deltaTime;
        }
    }
    
    public void ApplyStun(float duration)
    {
        stunTimer = duration;
    }
    
}