using UnityEngine;

public class EnemyHealth : MonoBehaviour, IHasHealth, IHittable, IDropper
{
    public int maxHealth = 3;
    public int currentHealth;

    public float invincibilityTime = 0.2f;
    private float invincibilityTimer;
    
    public LootTable lootTable;

    private EnemyMove move;

    void Awake()
    {
        currentHealth = maxHealth;
        move = GetComponent<EnemyMove>();
    }

    void Update()
    {
        if (invincibilityTimer > 0)
            invincibilityTimer -= Time.deltaTime;
    }

    public int getMaxHealth() => maxHealth;
    public int getCurrentHealth() => currentHealth;
    public void OnHit(DamageInfo info) => TakeDamage(info);

    public void TakeDamage(DamageInfo info)
    {
        if (invincibilityTimer > 0) return;

        currentHealth -= info.damage;
        invincibilityTimer = invincibilityTime;

        
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
        Drop();
        Debug.Log($"{name} died");
        Destroy(gameObject);
    }

    public void Drop()
    {
        if (lootTable != null)
            lootTable.Drop(transform.position);
    }
}