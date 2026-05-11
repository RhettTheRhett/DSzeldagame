using UnityEngine;

public interface IHasHealth 
{
    int getMaxHealth();
    int getCurrentHealth();

    void TakeDamage(DamageInfo info);
    void Heal(int amount);
    void Die();

}
