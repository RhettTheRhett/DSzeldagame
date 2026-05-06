using UnityEngine;

public class TestEnemyMove : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float rotateSpeed = 5f;

    [Header("Wander")]
    public float wanderRadius = 5f;
    public float wanderInterval = 5f;
    private Vector3 spawnPoint;
    private Vector3 wanderTarget;
    private float wanderTimer;

    [Header("Detection")]
    public float agroRange = 5f;
    public float memoryTime = 2f;
    private float loseSightTimer;
    private Transform player;

    [Header("Collision")]
    public float playerRadius = 0.25f;
    public float playerHeight = 0.5f;
    public float maxSlopeAngle = 45f;
    public float skinWidth = 0.05f;

    [Header("Physics")]
    public float gravity = -19.8f;
    private float verticalVelocity;

    // Knockback + stun
    private Vector3 knockbackVelocity;
    private float knockbackTimer;
    public float knockbackDecay = 10f;

    private float stunTimer;

    private void Start()
    {
        spawnPoint = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        PickNewWanderTarget();
    }

    private void FixedUpdate()
    {
        // -------- STUN --------
        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            return;
        }

        // -------- KNOCKBACK --------
        if (knockbackTimer > 0)
        {
            HandleKnockback();
            HandleGravity();
            return;
        }

        // -------- NORMAL BEHAVIOR --------
        if (player != null)
        {
            if (IsPlayerVisible())
            {
                loseSightTimer = memoryTime;
                MoveTowards(player.position);
            }
            else if (loseSightTimer > 0)
            {
                loseSightTimer -= Time.deltaTime;
                MoveTowards(player.position);
            }
            else
            {
                HandleWander();
            }
        }
        else
        {
            HandleWander();
        }

        HandleGravity();
    }

    // ======================
    // WANDER
    // ======================
    private void HandleWander()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0 || Vector3.Distance(transform.position, wanderTarget) < 0.5f)
        {
            PickNewWanderTarget();
        }

        MoveTowards(wanderTarget);
    }

    private void PickNewWanderTarget()
    {
        Vector2 random = Random.insideUnitCircle * wanderRadius;
        wanderTarget = spawnPoint + new Vector3(random.x, 0, random.y);
        wanderTimer = wanderInterval;
    }

    // ======================
    // MOVEMENT CORE
    // ======================
    private void MoveTowards(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0;

        if (dir.magnitude < 0.1f) return;

        dir.Normalize();

        float moveDistance = moveSpeed * Time.deltaTime;

        // 🚫 BLOCKED → try sliding
        if (IsBlocked(dir, moveDistance))
        {
            Vector3 dirX = new Vector3(dir.x, 0, 0).normalized;
            if (dir.x != 0 && !IsBlocked(dirX, moveDistance))
            {
                MoveOnSurface(dirX, moveDistance);
                Rotate(dirX);
                return;
            }

            Vector3 dirZ = new Vector3(0, 0, dir.z).normalized;
            if (dir.z != 0 && !IsBlocked(dirZ, moveDistance))
            {
                MoveOnSurface(dirZ, moveDistance);
                Rotate(dirZ);
                return;
            }

            return; // fully blocked
        }

        MoveOnSurface(dir, moveDistance);
        Rotate(dir);
    }

    private void Rotate(Vector3 dir)
    {
        transform.forward = Vector3.Slerp(
            transform.forward,
            dir,
            Time.deltaTime * rotateSpeed
        );
    }

    private bool IsBlocked(Vector3 dir, float distance)
    {
        // ✅ Correct capsule (center-safe)
        Vector3 bottom = transform.position + Vector3.up * playerRadius;
        Vector3 top = transform.position + Vector3.up * (playerHeight - playerRadius);

        return Physics.CapsuleCast(
            bottom,
            top,
            playerRadius,
            dir,
            distance + skinWidth
        );
    }

    private void MoveOnSurface(Vector3 dir, float distance)
    {
        if (IsGrounded(out RaycastHit hit))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (slopeAngle <= maxSlopeAngle)
            {
                Vector3 slopeDir = Vector3.ProjectOnPlane(dir, hit.normal).normalized;
                transform.position += slopeDir * distance;
                return;
            }
        }

        transform.position += dir * distance;
    }

    // ======================
    // GRAVITY
    // ======================
    private void HandleGravity()
    {
        if (IsGrounded(out RaycastHit hit))
        {
            verticalVelocity = 0f;

            float groundY = hit.point.y + playerHeight;
            transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
            transform.position += Vector3.up * (verticalVelocity * Time.deltaTime);
        }
    }

    private bool IsGrounded(out RaycastHit hit)
    {
        return Physics.Raycast(transform.position, Vector3.down, out hit, playerHeight + 0.1f);
    }

    // ======================
    // LINE OF SIGHT
    // ======================
    private bool IsPlayerVisible()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * (playerHeight * 0.5f);
        Vector3 target = player.position + Vector3.up * 0.5f;

        Vector3 dir = target - origin;
        float dist = dir.magnitude;

        if (dist > agroRange) return false;

        dir.Normalize();

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
        {
            return hit.transform == player;
        }

        return false;
    }

    // ======================
    // KNOCKBACK + STUN
    // ======================
    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        knockbackVelocity = direction.normalized * force;
        knockbackTimer = duration;
    }

    public void ApplyStun(float duration)
    {
        stunTimer = duration;
    }

    private void HandleKnockback()
    {
        float moveDistance = knockbackVelocity.magnitude * Time.deltaTime;
        Vector3 dir = knockbackVelocity.normalized;

        if (!IsBlocked(dir, moveDistance))
        {
            MoveOnSurface(dir, moveDistance);
        }

        knockbackVelocity = Vector3.Lerp(
            knockbackVelocity,
            Vector3.zero,
            knockbackDecay * Time.deltaTime
        );

        knockbackTimer -= Time.deltaTime;
    }

    // ======================
    // DEBUG GIZMOS
    // ======================
    private void OnDrawGizmosSelected()
    {
        // -------- Wander Area --------
        Gizmos.color = Color.green;
        Vector3 center = Application.isPlaying ? spawnPoint : transform.position;
        Gizmos.DrawWireSphere(center, wanderRadius);

        // -------- Aggro Range --------
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, agroRange);

        // -------- Wander Target --------
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(wanderTarget, 0.2f);
            Gizmos.DrawLine(transform.position, wanderTarget);
        }

        // -------- CAPSULE DEBUG --------
        Gizmos.color = Color.red;

        float radius = playerRadius;
        float height = playerHeight;

        Vector3 bottom = transform.position + Vector3.up * radius;
        Vector3 top = transform.position + Vector3.up * (height - radius);

        // Draw spheres (caps)
        Gizmos.DrawWireSphere(bottom, radius);
        Gizmos.DrawWireSphere(top, radius);

        // Draw sides
        Gizmos.DrawLine(bottom + Vector3.forward * radius, top + Vector3.forward * radius);
        Gizmos.DrawLine(bottom - Vector3.forward * radius, top - Vector3.forward * radius);
        Gizmos.DrawLine(bottom + Vector3.right * radius, top + Vector3.right * radius);
        Gizmos.DrawLine(bottom - Vector3.right * radius, top - Vector3.right * radius);
    }
}