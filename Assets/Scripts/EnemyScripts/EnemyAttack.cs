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

    // How long the enemy waits before attacking after FIRST entering attack range.
    // Prevents the enemy from instantly hitting the player on contact.
    public float firstAttackDelay = 0.6f;

    private float lastAttackTime;
    private Transform player;
    private IHasHealth playerHealth;

    // Tracks whether we've entered attack range since last exit
    private bool inRangeLastFrame = false;
    // Timer for the initial entry delay
    private float entryDelayTimer = 0f;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<IHasHealth>();
        }
    }

    void Update()
    {
        if (player == null || playerHealth == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool inRange = distance <= attackRange;

        if (inRange)
        {
            if (!inRangeLastFrame)
            {
                // Just entered range — start the entry delay
                entryDelayTimer = firstAttackDelay;
            }

            if (entryDelayTimer > 0)
            {
                entryDelayTimer -= Time.deltaTime;
            }
            else
            {
                TryAttack();
            }
        }
        else
        {
            // Reset entry delay when player leaves range
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
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}