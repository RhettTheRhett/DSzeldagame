using UnityEngine;

public class EnemyHealth : MonoBehaviour, IHasHealth
{
    public int maxHealth = 3;
    public int currentHealth;

    // Briefly prevents the same hit from registering twice on the same frame
    public float invincibilityTime = 0.2f;
    private float invincibilityTimer;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (invincibilityTimer > 0)
            invincibilityTimer -= Time.deltaTime;
    }

    public int getMaxHealth() => maxHealth;
    public int getCurrentHealth() => currentHealth;

    public void TakeDamage(DamageInfo info)
    {
        if (invincibilityTimer > 0) return;

        currentHealth -= info.damage;
        invincibilityTimer = invincibilityTime;

        // Works with either EnemyMove (Rigidbody) automatically
        EnemyMove move = GetComponent<EnemyMove>();
        if (move != null)
        {
            Vector3 dir = transform.position - info.sourcePosition;
            dir.y = 0;
            dir.Normalize();
            move.ApplyKnockback(dir, info.knockbackForce, info.knockbackDuration);
            move.ApplyStun(info.stunDuration);
        }

        Debug.Log($"{name} took {info.damage} damage. HP: {currentHealth}");

        if (currentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public void Die()
    {
        Debug.Log($"{name} died");
        Destroy(gameObject);
    }
}