using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [Header("Potion Settings")]
    [SerializeField]
    private int healAmount = 1;
    [SerializeField]
    private GameObject pickupEffectPrefab; // Optional: particle effect on pickup

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object that entered is the player
        if (other.CompareTag("Player"))
        {
            // Try to get the Health component from the player
            if (other.TryGetComponent<Health>(out Health playerHealth))
            {
                // Heal the player
                playerHealth.Heal(healAmount);

                // Play a pickup effect if one is assigned
                if (pickupEffectPrefab != null)
                {
                    Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                }

                // Destroy the potion object
                Destroy(gameObject);
            }
        }
    }
}