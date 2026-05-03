using UnityEngine;

public struct DamageInfo
{
    public int damage;
    public Vector3 sourcePosition;
    public float knockbackForce;
    public float knockbackDuration;
    public float stunDuration;

    public DamageInfo(int damage, Vector3 sourcePosition, float force, float kbDuration, float stun)
    {
        this.damage = damage;
        this.sourcePosition = sourcePosition;
        this.knockbackForce = force;
        this.knockbackDuration = kbDuration;
        this.stunDuration = stun;
    }
}