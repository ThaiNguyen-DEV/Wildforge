using System.Collections;
using UnityEngine;

public class AliveFlashFeedback : MonoBehaviour
{
    [Header("Feedback Settings")]
    [SerializeField]
    private Material flashMaterial; // Assign your GUI/TextShader material here
    [SerializeField]
    private float flashDuration = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Health health;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        // Get the SpriteRenderer on this child object
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Find the Health component in the parent GameObject
        health = GetComponentInParent<Health>();

        // Store the original material
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
        }
        else
        {
            Debug.LogWarning("AliveFlashFeedback requires a SpriteRenderer component on the same GameObject.", this);
        }

        if (health == null)
        {
            Debug.LogError("AliveFlashFeedback could not find a Health component in any parent GameObject.", this);
        }
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnHit += TriggerFlash;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnHit -= TriggerFlash;
        }
    }

    private void TriggerFlash()
    {
        if (spriteRenderer == null || flashMaterial == null) return;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            spriteRenderer.material = originalMaterial;
        }
        
        flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        spriteRenderer.material = flashMaterial;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.material = originalMaterial;
        flashCoroutine = null;
    }
}