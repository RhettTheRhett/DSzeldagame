using UnityEngine;

public class HeartIncreasePickup : MonoBehaviour, IInteractable
{
    public int healthIncreaseAmount = 1;
    
    public void Interacted(GameObject interactor)
    {
        PlayerHealth playerHealth = interactor.GetComponent<PlayerHealth>();

        if (playerHealth)
        {
            playerHealth.IncreaseMaxHealth(healthIncreaseAmount);
            Destroy(gameObject);
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }
}
