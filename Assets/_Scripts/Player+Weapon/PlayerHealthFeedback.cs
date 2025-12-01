using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerHealthFeedback : MonoBehaviour
{
    private Health health;
    private int lastKnownHealth;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        health.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        health.OnHealthChanged -= HandleHealthChanged;
    }

    private void Start()
    {
        // Initialize with current health
        lastKnownHealth = health.GetCurrentHealth();
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        // Check if health has decreased
        if (currentHealth < lastKnownHealth)
        {
            // Trigger UI flash feedback
            if (UIManager.Instance != null)
            {
                UIManager.Instance.FlashHealthUI();
            }
        }

        // Update last known health
        lastKnownHealth = currentHealth;
    }
}