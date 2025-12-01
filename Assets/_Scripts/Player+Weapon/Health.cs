using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField]
    private int maxHealth = 3;
    private int currentHealth;

    [SerializeField]
    private int scoreValue = 50;

    public event Action OnDeath;
    public event Action<int, int> OnHealthChanged;
    public event Action OnHit; // New event for feedback

    private KnockbackFeedback knockbackFeedback;

    private void Awake()
    {
        currentHealth = maxHealth;
        knockbackFeedback = GetComponent<KnockbackFeedback>();
    }

    private void Start()
    {
        // Broadcast initial health state
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Allows external UI scripts to request the current health state upon initialization.
    /// </summary>
    public void RequestInitialHealth()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void GetHit(int damage, GameObject sender)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // Invoke events
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnHit?.Invoke(); // Trigger the hit feedback

        if (knockbackFeedback != null)
        {
            knockbackFeedback.PlayFeedback(sender);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }
        gameObject.SetActive(false);
    }
}
