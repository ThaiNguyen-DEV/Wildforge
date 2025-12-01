using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class FlashFeedback : MonoBehaviour
{
    [Header("Feedback Settings")]
    [SerializeField]
    private Material flashMaterial;
    [SerializeField]
    private float flashDuration = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Health health;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = GetComponent<Health>();

        // Store the original material
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
        }
    }

    private void OnEnable()
    {
        if (health != null)
        {
            // Subscribe to the OnHit event
            health.OnHit += TriggerFlash;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            // Unsubscribe from the OnHit event
            health.OnHit -= TriggerFlash;
        }
    }

    private void TriggerFlash()
    {
        // If a flash is already happening, stop it and start a new one
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            // Ensure we revert to original material before starting a new flash
            if (spriteRenderer != null)
            {
                spriteRenderer.material = originalMaterial;
            }
        }
        flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        // Swap to the flash material
        if (spriteRenderer != null && flashMaterial != null)
        {
            spriteRenderer.material = flashMaterial;
        }

        // Wait for the flash duration
        yield return new WaitForSeconds(flashDuration);

        // Revert to the original material
        if (spriteRenderer != null)
        {
            spriteRenderer.material = originalMaterial;
        }
        flashCoroutine = null;
    }
}