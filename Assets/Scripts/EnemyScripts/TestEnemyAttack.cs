using UnityEngine;

public class TestEnemyAttack : MonoBehaviour
{
    public int damage = 1;

    public float attackCooldown = 1.5f;
    public float attackRange = 1.5f;
    public float agroRange = 5f;

    public float knockbackForce = 3f;
    public float knockbackDuration = 0.15f;

    private float lastAttackTime;
    private Transform target;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    void Update()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= agroRange)
        {
            if (distance > attackRange)
            {
                MoveTowardsTarget();
            }
            else
            {
                TryAttack();
            }
        }
    }

    private void MoveTowardsTarget()
    {
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * (1.25f * Time.deltaTime);
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        IHasHealth health = target.GetComponent<IHasHealth>();
        if (health != null)
        {
            DamageInfo info = new DamageInfo(
                damage,
                transform.position,
                knockbackForce,
                knockbackDuration,
                0.2f
            );

            health.TakeDamage(info);
            lastAttackTime = Time.time;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // -------- Attack Range (Red) --------
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}