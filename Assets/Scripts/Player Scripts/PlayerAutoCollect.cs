using UnityEngine;

public class PlayerAutoCollect : MonoBehaviour
{
    // Fires when the player walks into any trigger collider
    // that has an IAutoCollect component
    private void OnTriggerEnter(Collider other)
    {
        IAutoCollect collectible = other.GetComponentInParent<IAutoCollect>();
        collectible?.Collect(gameObject);
    }
}