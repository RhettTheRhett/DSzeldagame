using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TestEnemyMoveV2 : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 10f;

    [Header("Wander")]
    public float wanderRadius = 5f;
    public float wanderInterval = 3f;
    private Vector3 spawnPoint;
    private Vector3 wanderTarget;
    private float wanderTimer;

    [Header("Detection")]
    public float agroRange = 5f;
    public float memoryTime = 2f;
    private float loseSightTimer;
    private Transform player;
    
    [Header("Wall Detection")]
    public float wallCheckDistance = 0.6f;
    public float wallCheckHeight = 0.5f;

    private Rigidbody rb;

    // Knockback + stun
    private float stunTimer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
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
            stunTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        Vector3 moveDir = Vector3.zero;

        // -------- BEHAVIOR --------
        if (player != null)
        {
            if (IsPlayerVisible())
            {
                loseSightTimer = memoryTime;
                moveDir = (player.position - transform.position);
            }
            else if (loseSightTimer > 0)
            {
                loseSightTimer -= Time.fixedDeltaTime;
                moveDir = (player.position - transform.position);
            }
            else
            {
                moveDir = HandleWander();
            }
        }
        else
        {
            moveDir = HandleWander();
        }

        Move(moveDir);
    }

    // ======================
    // MOVEMENT
    // ======================
    private void Move(Vector3 dir)
    {
        dir.y = 0;

        if (dir.magnitude < 0.1f)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        dir.Normalize();

       
        if (IsWallAhead(dir))
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        rb.linearVelocity = new Vector3(
            dir.x * moveSpeed,
            rb.linearVelocity.y,
            dir.z * moveSpeed
        );

        transform.forward = Vector3.Slerp(
            transform.forward,
            dir,
            Time.fixedDeltaTime * rotateSpeed
        );
    }

    // ======================
    // WANDER
    // ======================
    private Vector3 HandleWander()
    {
        wanderTimer -= Time.fixedDeltaTime;

        if (wanderTimer <= 0 || Vector3.Distance(transform.position, wanderTarget) < 1f)
        {
            PickNewWanderTarget();
        }

        return wanderTarget - transform.position;
    }

    private void PickNewWanderTarget()
    {
        Vector2 random = Random.insideUnitCircle * wanderRadius;
        wanderTarget = spawnPoint + new Vector3(random.x, 0, random.y);
        wanderTimer = wanderInterval;
    }

    // ======================
    // LINE OF SIGHT
    // ======================
    private bool IsPlayerVisible()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
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
    
    private bool IsWallAhead(Vector3 dir)
    {
        Vector3 origin = transform.position + Vector3.up * wallCheckHeight;

        return Physics.Raycast(origin, dir, wallCheckDistance);
    }

    // ======================
    // KNOCKBACK + STUN
    // ======================
    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        direction.y = 0;
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
    }

    public void ApplyStun(float duration)
    {
        stunTimer = duration;
    }

    // ======================
    // DEBUG
    // ======================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = Application.isPlaying ? spawnPoint : transform.position;
        Gizmos.DrawWireSphere(center, wanderRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, agroRange);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(wanderTarget, 0.2f);
        }
        
        Gizmos.color = Color.red;
        Vector3 origin = transform.position + Vector3.up * wallCheckHeight;
        Gizmos.DrawLine(origin, origin + transform.forward * wallCheckDistance);
    }
}