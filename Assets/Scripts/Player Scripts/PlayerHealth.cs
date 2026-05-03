using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IHasHealth
{
    
    public int maxHealth;
    public int currentHealth;
    
    public System.Action<int, int> OnHealthChanged;
    public System.Action OnHurt;
    
    public float invincibilityTime = 0.5f;
    private float invincibilityTimer;
    
    void Awake()
    {
        currentHealth =  maxHealth;
    }

    private void Update()
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

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnHurt?.Invoke();

        PlayerMove move = GetComponent<PlayerMove>();
        if (move)
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
        currentHealth += amount;
        //Debug.Log(name + " healed " + amount + " health. HP: " + currentHealth);
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Die()
    {
        Debug.Log(name + "died");
        //Destroy(gameObject);
    }
}
