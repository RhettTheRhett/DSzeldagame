using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    
    public PlayerHealth playerHealth;

    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;
    
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateUI;
                
                UpdateUI(playerHealth.getCurrentHealth(), playerHealth.getMaxHealth());
            }
        }
    }

    
    private void UpdateUI(int current, int max)
    {
        //Debug.Log("Update UI: " + current + "/" + max);

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < current)
            {
                hearts[i].sprite = fullHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }

            if (i < max)
            {
                hearts[i].enabled = true;
            }
            else
            {
                hearts[i].enabled = false;
            }
        }
    }
    
    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateUI;
        }
    }
}
