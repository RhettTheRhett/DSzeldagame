using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack")]
    public int damage = 1;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public float knockbackForce = 3f;
    public float knockbackDuration = 0.15f;
    public float stunDuration = 0.2f;

    // How long the enemy waits before attacking after entering attack range.
    public float firstAttackDelay = 0.6f;

    // How long the enemy stands still after landing an attack before
    // moving again. Gives attacks visual weight and prevents jittering
    // in and out of range immediately after hitting.
    public float postAttackPause = 0.5f;

    private float lastAttackTime;
    private Transform player;
    private IHasHealth playerHealth;

    private bool inRangeLastFrame = false;
    private float entryDelayTimer = 0f;
    private float postAttackPauseTimer = 0f;

    // EnemyMove polls these to decide whether to stop moving
    public bool IsInAttackRange => player != null &&
        Vector3.Distance(transform.position, player.position) <= attackRange;
    public bool ShouldStopMoving => IsInAttackRange || postAttackPauseTimer > 0;

    void Start()
    {
        player = PlayerRef.Transform;
        playerHealth = PlayerRef.Health;
    }

    void Update()
    {
        if (player == null || playerHealth == null) return;

        if (postAttackPauseTimer > 0)
        {
            postAttackPauseTimer -= Time.deltaTime;
            return;
        }

        bool inRange = IsInAttackRange;

        if (inRange)
        {
            if (!inRangeLastFrame)
                entryDelayTimer = firstAttackDelay;

            if (entryDelayTimer > 0)
                entryDelayTimer -= Time.deltaTime;
            else
                TryAttack();
        }
        else
        {
            entryDelayTimer = 0f;
        }

        inRangeLastFrame = inRange;
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        DamageInfo info = new DamageInfo(
            damage,
            transform.position,
            knockbackForce,
            knockbackDuration,
            stunDuration
        );

        playerHealth.TakeDamage(info);
        lastAttackTime = Time.time;

        // Start the post-attack pause so the enemy doesn't immediately
        // shuffle back into range and attack again
        postAttackPauseTimer = postAttackPause;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}