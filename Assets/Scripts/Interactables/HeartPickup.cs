using UnityEngine;

public class HeartPickup : MonoBehaviour, IAutoCollect
{
    public int healAmount = 1;

    [Header("Ground Snap")]
    // How far down to search for the floor
    public float groundCheckDistance = 2f;
    // How high above the floor the pivot sits.
    // Adjust to match the visual center of your heart mesh.
    public float groundOffset = 0.1f;

    void Start()
    {
        SnapToGround();
    }

    private void SnapToGround()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance))
        {
            transform.position = new Vector3(
                transform.position.x,
                hit.point.y + groundOffset,
                transform.position.z
            );
        }
    }

    public void Collect(GameObject collector)
    {
        IHasHealth health = collector.GetComponent<IHasHealth>();
        if (health == null) return;
        if (health.getCurrentHealth() == health.getMaxHealth()) return;

        health.Heal(healAmount);
        Destroy(gameObject);
    }
}