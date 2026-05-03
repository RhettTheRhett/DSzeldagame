using UnityEngine;
using UnityEngine.UI;

public class HurtFlashUI : MonoBehaviour
{
    public PlayerHealth playerHealth;

    public Image hurtFlashImage;
    public float flashDuration = 0.2f;

    private float flashTimer;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        

        if (player != null)
        {
            //Debug.Log("player found");
            playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                //Debug.Log("player health found");
                playerHealth.OnHurt += HurtFlash;
                //Debug.Log("event subscribed");
            }
        }

        hurtFlashImage.enabled = false;
    }

    private void HurtFlash()
    {
        flashTimer = flashDuration;
        hurtFlashImage.enabled = true;
        //Debug.Log("image enabled");
    }

    void Update()
    {
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;

            float alpha = flashTimer / flashDuration;

            Color c = hurtFlashImage.color;
            c.a = alpha;
            hurtFlashImage.color = c;

            if (flashTimer <= 0)
            {
                hurtFlashImage.enabled = false;
            }
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHurt -= HurtFlash;
        }
    }
}