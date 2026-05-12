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
    public float stunDuration = 0.75f;

    private Animator animator;
    
    private void Start()
    {
        attackAction = InputSystem.actions.FindAction("Attack");
        
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (attackAction.WasPressedThisFrame())
        {
            animator.SetTrigger("Slash");
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

            IHittable hittable = col.GetComponentInParent<IHittable>();

            if (hittable != null)
            {
                DamageInfo info = new DamageInfo(damage, transform.position, knockbackForce, knockbackDuration, stunDuration);

                hittable.OnHit(info);
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