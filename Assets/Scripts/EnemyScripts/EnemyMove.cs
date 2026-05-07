using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float rotateSpeed = 5f;

    [Header("Capsule - assumes transform.position is at character center")]
    public float characterRadius = 0.25f;
    public float characterHeight = 2f;

    [Header("Slope & Gravity")]
    public float maxSlopeAngle = 45f;
    public float gravity = -19.8f;
    public float groundCheckTolerance = 0.15f;
    public float skinWidth = 0.05f;

    [Header("Wander")]
    public float wanderRadius = 5f;
    public float wanderInterval = 5f;
    public float wanderArrivalThreshold = 1f;

    [Header("Detection")]
    public float agroRange = 5f;
    public float memoryTime = 2f;
    public float eyeHeightOffset = 0f;

    [Header("Knockback")]
    public float knockbackDecay = 10f;

    private Transform player;
    private Vector3 spawnPoint;
    private Vector3 wanderTarget;
    private float wanderTimer;
    private float loseSightTimer;
    private float verticalVelocity;

    private Vector3 knockbackVelocity;
    private float knockbackTimer;
    private float stunTimer;

    private float HalfHeight => characterHeight * 0.5f;

    private void Start()
    {
        spawnPoint = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        PickNewWanderTarget();
    }

    private void FixedUpdate()
    {
        if (stunTimer > 0)
        {
            stunTimer -= Time.fixedDeltaTime;
            HandleGravity();
            return;
        }

        if (knockbackTimer > 0)
        {
            HandleKnockback();
            HandleGravity();
            return;
        }

        Vector3 moveDir = Vector3.zero;

        if (player != null)
        {
            if (IsPlayerVisible())
            {
                loseSightTimer = memoryTime;
                moveDir = player.position - transform.position;
            }
            else if (loseSightTimer > 0)
            {
                loseSightTimer -= Time.fixedDeltaTime;
                moveDir = player.position - transform.position;
            }
            else
            {
                moveDir = GetWanderDirection();
            }
        }
        else
        {
            moveDir = GetWanderDirection();
        }

        HandleMove(moveDir);

        // Gravity always runs last and always wins — nothing after this
        // should modify transform.position.y
        HandleGravity();
    }

    // =====================
    // MOVEMENT
    // =====================
    private void HandleMove(Vector3 dir)
    {
        dir.y = 0;
        if (dir.magnitude < 0.1f) return;
        dir.Normalize();

        float distance = moveSpeed * Time.fixedDeltaTime;

        if (!IsBlocked(dir, distance))
        {
            MoveOnSurface(dir, distance);
            Rotate(dir);
            return;
        }

        Vector3 dirX = new Vector3(dir.x, 0, 0).normalized;
        if (dir.x != 0 && !IsBlocked(dirX, distance))
        {
            MoveOnSurface(dirX, distance);
            Rotate(dirX);
            return;
        }

        Vector3 dirZ = new Vector3(0, 0, dir.z).normalized;
        if (dir.z != 0 && !IsBlocked(dirZ, distance))
        {
            MoveOnSurface(dirZ, distance);
            Rotate(dirZ);
        }
    }

    private void MoveOnSurface(Vector3 dir, float distance)
    {
        if (IsGrounded(out RaycastHit hit))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (slopeAngle <= maxSlopeAngle)
            {
                Vector3 slopeDir = Vector3.ProjectOnPlane(dir, hit.normal).normalized;
                Vector3 newPos = transform.position + slopeDir * distance;

                // Clamp Y — slope projection can introduce a tiny negative Y
                // on flat surfaces due to floating point, which accumulates
                // and eventually sinks the character through the floor
                newPos.y = Mathf.Max(newPos.y, transform.position.y);

                transform.position = newPos;
                return;
            }
        }
        // Only move horizontally if not on a valid slope
        Vector3 flatMove = new Vector3(dir.x, 0, dir.z) * distance;
        transform.position += flatMove;
    }

    private void Rotate(Vector3 dir)
    {
        transform.forward = Vector3.Slerp(transform.forward, dir, Time.fixedDeltaTime * rotateSpeed);
    }

    // =====================
    // COLLISION
    // =====================
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
        return Physics.Raycast(
            transform.position,
            Vector3.down,
            out hit,
            HalfHeight + groundCheckTolerance
        );
    }

    // =====================
    // LINE OF SIGHT
    // =====================
    private bool IsPlayerVisible()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * eyeHeightOffset;
        Vector3 target = player.position + Vector3.up * eyeHeightOffset;
        Vector3 dir = target - origin;
        float dist = dir.magnitude;

        if (dist > agroRange) return false;

        dir.Normalize();
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
            return hit.transform == player;

        return true;
    }

    // =====================
    // WANDER
    // =====================
    private Vector3 GetWanderDirection()
    {
        wanderTimer -= Time.fixedDeltaTime;

        if (wanderTimer <= 0 || Vector3.Distance(transform.position, wanderTarget) < wanderArrivalThreshold)
            PickNewWanderTarget();

        return wanderTarget - transform.position;
    }

    private void PickNewWanderTarget()
    {
        Vector2 random = Random.insideUnitCircle * wanderRadius;
        wanderTarget = spawnPoint + new Vector3(random.x, 0, random.y);
        wanderTimer = wanderInterval;
    }

    // =====================
    // KNOCKBACK + STUN
    // =====================
    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        direction.y = 0;
        knockbackVelocity = direction.normalized * force;
        knockbackTimer = duration;
    }

    public void ApplyStun(float duration)
    {
        stunTimer = duration;
    }

    private void HandleKnockback()
    {
        Vector3 dir = knockbackVelocity.normalized;
        float distance = knockbackVelocity.magnitude * Time.fixedDeltaTime;

        if (!IsBlocked(dir, distance))
            MoveOnSurface(dir, distance);

        knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDecay * Time.fixedDeltaTime);
        knockbackTimer -= Time.fixedDeltaTime;
    }

    // =====================
    // GIZMOS
    // =====================
    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? spawnPoint : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, wanderRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, agroRange);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(wanderTarget, 0.2f);
            Gizmos.DrawLine(transform.position, wanderTarget);
        }

        Gizmos.color = Color.red;
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

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(
            transform.position,
            transform.position + Vector3.down * (HalfHeight + groundCheckTolerance)
        );
    }
}