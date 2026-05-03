using UnityEngine;

public class HeartPickup : MonoBehaviour, IInteractable
{
    public int healAmount = 1;
    public void Interacted(GameObject interactor)
    {
        IHasHealth playerHealth = interactor.GetComponent<IHasHealth>();

        if (playerHealth != null)
        {
            if (playerHealth.getCurrentHealth() != playerHealth.getMaxHealth())
            {
                playerHealth.Heal(Mathf.Min(healAmount, playerHealth.getMaxHealth()));
                Destroy(gameObject);
            }
        }
        
        
    }

    public Transform GetTransform()
    {
        return transform;
    }
}
