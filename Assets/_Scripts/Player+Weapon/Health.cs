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
    public event Action OnHit;

    private KnockbackFeedback knockbackFeedback;

    private void Awake()
    {
        currentHealth = maxHealth;
        knockbackFeedback = GetComponent<KnockbackFeedback>();
    }

    private void Start()
    {
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

    public void RequestInitialHealth()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void GetHit(int damage, GameObject sender)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnHit?.Invoke();

        if (knockbackFeedback != null)
        {
            knockbackFeedback.PlayFeedback(sender);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Heals the character by a given amount, clamping at max health.
    /// </summary>
    /// <param name="amount">The amount of health to restore.</param>
    public void Heal(int amount)
    {
        if (currentHealth <= 0) return; // Can't heal if dead

        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        // Notify UI and other systems of the health change
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
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
