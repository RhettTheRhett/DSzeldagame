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
    private Transform player;

    [Header("Collision")]
    public float playerRadius = 0.25f;
    public float playerHeight = 0.5f;
    public float maxSlopeAngle = 45f;

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
        HandleKnockback();

        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            return;
        }

        if (knockbackTimer > 0) return;

        if (player != null && Vector3.Distance(transform.position, player.position) <= agroRange)
        {
            MoveTowards(player.position);
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
        Vector3 dir = (target - transform.position);
        dir.y = 0;

        if (dir.magnitude < 0.1f) return;

        dir.Normalize();

        float moveDistance = moveSpeed * Time.deltaTime;

        if (!IsBlocked(dir, moveDistance))
        {
            MoveOnSurface(dir, moveDistance);
        }

        // Rotate
        transform.forward = Vector3.Slerp(
            transform.forward,
            dir,
            Time.deltaTime * rotateSpeed
        );
    }

    private bool IsBlocked(Vector3 dir, float distance)
    {
        return Physics.CapsuleCast(
            transform.position,
            transform.position + Vector3.up * playerHeight,
            playerRadius,
            dir,
            distance
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
    
    private void OnDrawGizmosSelected()
    {
        // -------- Wander Area (Green) --------
        Gizmos.color = Color.green;
        Vector3 center = Application.isPlaying ? spawnPoint : transform.position;
        Gizmos.DrawWireSphere(center, wanderRadius);

        // -------- Aggro Range (Yellow) --------
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, agroRange);

        // -------- Current Wander Target (Blue) --------
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(wanderTarget, 0.2f);
            Gizmos.DrawLine(transform.position, wanderTarget);
        }
    }
}