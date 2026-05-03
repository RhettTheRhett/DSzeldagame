using UnityEngine;

public class EnemyHealth : MonoBehaviour, IHasHealth
{
    public int maxHealth;
    public int currentHealth;
    
    public float invincibilityTime = 0.2f;
    private float invincibilityTimer;
    
    void Awake()
    {
        currentHealth =  maxHealth;
    }

    void Update()
    {
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }
    
    public int getMaxHealth()
    {
        return maxHealth;
    }

    public int getCurrentHealth()
    {
        return currentHealth;
    }

    public void TakeDamage(DamageInfo info)
    {
        if (invincibilityTimer > 0) return;

        currentHealth -= info.damage;

        var move = GetComponent<TestEnemyMove>(); // replace if needed
        if (move != null)
        {
            Vector3 dir = (transform.position - info.sourcePosition);
            dir.y = 0;
            dir.Normalize();

            move.ApplyKnockback(dir, info.knockbackForce, info.knockbackDuration);
            move.ApplyStun(info.stunDuration);
        }

        invincibilityTimer = invincibilityTime;

        Debug.Log(name + " took " + info.damage + " damage. HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        throw new System.NotImplementedException();
    }

    public void Die()
    {
        Debug.Log(name + "died");
        Destroy(gameObject);
    }
}
