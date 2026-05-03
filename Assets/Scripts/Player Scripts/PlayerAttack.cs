using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public InputAction attackAction;

    public Vector3 halfExtents = new Vector3(0.5f, 0.5f, 0.5f);
    public float maxDistance = 1f;

    public int damage = 1;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    private void Start()
    {
        attackAction = InputSystem.actions.FindAction("Attack");
    }

    private void Update()
    {
        if (attackAction.WasPressedThisFrame())
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        Vector3 origin = transform.position + Vector3.up * 0.25f;
        Vector3 center = origin + transform.forward * maxDistance;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);

        foreach (var col in hits)
        {
            if (col.transform.root == transform) continue;

            IHasHealth health = col.GetComponentInParent<IHasHealth>();

            if (health != null)
            {
                DamageInfo info = new DamageInfo(damage, transform.position, knockbackForce, knockbackDuration, 0.1f);

                health.TakeDamage(info);
            }
        }
    }
    
    private void OnDrawGizmos() { 
        Gizmos.color = Color.red; 
        Vector3 origin = transform.position + Vector3.up * 0.25f; 
        Vector3 direction = transform.forward; 
        Vector3 end = origin + direction * maxDistance; 
        // Draw start and end boxes
        Gizmos.matrix = Matrix4x4.TRS(origin, transform.rotation, Vector3.one); 
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f); 
        Gizmos.matrix = Matrix4x4.TRS(end, transform.rotation, Vector3.one); 
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f); 
        // Reset matrix
        Gizmos.matrix = Matrix4x4.identity;
        // // Draw connecting lines (edges of sweep)
        Gizmos.DrawLine(origin, end); }
}