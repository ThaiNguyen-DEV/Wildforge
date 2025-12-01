using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Agent : MonoBehaviour
{
    private AgentAnimations agentAnimations;
    private AgentMover agentMover;

    private Vector2 pointerInput, movementInput;

    public Vector2 PointerInput { get => pointerInput; set => pointerInput = value; }
    public Vector2 MovementInput { get => movementInput; set { if (!isDashing) movementInput = value; } }

    private WeaponBase weapon; // Changed from WeaponParent to WeaponBase

    // Input wiring (use custom PlayerInput, not InputSystem.PlayerInput)
    private global::PlayerInput playerInput;

    // UI cooldown handler
    private PlayerCooldownAndHealthUI cooldownUI;

    // Pause manager
    private PauseManager pauseManager;

    // Dash settings
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.15f;
    private bool isDashing;

    // Skill (projectile) settings
    [SerializeField] private GameObject projectilePrefab;

    public bool IsDashing => isDashing;

    private void Awake()
    {
        agentMover = GetComponent<AgentMover>();
        agentAnimations = GetComponentInChildren<AgentAnimations>();
        weapon = GetComponentInChildren<WeaponBase>(); // Changed to get WeaponBase
        playerInput = GetComponent<global::PlayerInput>();
        cooldownUI = GetComponent<PlayerCooldownAndHealthUI>();
        pauseManager = FindObjectOfType<PauseManager>();

        // Debug pause manager
        if (pauseManager == null)
        {
            Debug.LogError("PauseManager not found in scene! Make sure PauseManager GameObject exists.");
        }
        else
        {
            Debug.Log("PauseManager found successfully!");
        }
    }

    private void OnEnable()
    {
        if (playerInput == null) playerInput = GetComponent<global::PlayerInput>();
        if (playerInput != null)
        {
            playerInput.OnMovementInput.AddListener(OnMoveEvent);
            playerInput.OnPointerInput.AddListener(OnPointerEvent);
            playerInput.OnAttack.AddListener(PerformAttack);
            playerInput.OnDash.AddListener(PerformDash);
            playerInput.OnSkill.AddListener(PerformSkill);
            playerInput.OnPause.AddListener(PerformPause);
            Debug.Log("Agent: OnPause listener added");
        }
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.OnMovementInput.RemoveListener(OnMoveEvent);
            playerInput.OnPointerInput.RemoveListener(OnPointerEvent);
            playerInput.OnAttack.RemoveListener(PerformAttack);
            playerInput.OnDash.RemoveListener(PerformDash);
            playerInput.OnSkill.RemoveListener(PerformSkill);
            playerInput.OnPause.RemoveListener(PerformPause);
        }
    }

    private void OnMoveEvent(Vector2 v)
    {
        if (isDashing) return;
        MovementInput = v;
    }

    private void OnPointerEvent(Vector2 v)
    {
        PointerInput = v;
    }

    private void Update()
    {
        agentMover.MovementInput = MovementInput;

        if (weapon != null)
        {
            weapon.PointerPosition = pointerInput;
        }

        AnimateCharacter();
    }

    public void PerformAttack()
    {
        weapon?.Attack(); // Use the generic weapon reference
    }

    public void PerformDash()
    {
        // Check UI cooldown first
        if (cooldownUI != null && !cooldownUI.TryUseDash()) return;
        if (isDashing) return;

        Vector2 origin = transform.position;
        Vector2 dir = MovementInput.sqrMagnitude > 0.0001f
            ? MovementInput.normalized
            : (pointerInput - origin).sqrMagnitude > 0.0001f
                ? (pointerInput - origin).normalized
                : Vector2.right;

        StartCoroutine(DashRoutine(dir));
    }

    private IEnumerator DashRoutine(Vector2 dir)
    {
        isDashing = true;

        Vector2 previous = MovementInput;
        movementInput = Vector2.zero;

        agentMover.SetForcedVelocity(dir * dashSpeed, dashDuration);

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        movementInput = previous;
    }

    public void PerformSkill()
    {
        // Check UI cooldown first
        if (cooldownUI != null && !cooldownUI.TryUseSkill()) return;
        if (projectilePrefab == null) return;

        // Determine spawn position and direction
        Vector2 spawnPosition = weapon != null ? (Vector2)weapon.transform.position : (Vector2)transform.position;
        Vector2 direction = (pointerInput - spawnPosition).normalized;

        if (direction.sqrMagnitude < 0.01f)
        {
            direction = transform.right; // Default direction if pointer is too close
        }

        // Instantiate and launch the projectile
        GameObject projectileInstance = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        projectileInstance.GetComponent<Projectile>()?.Launch(direction, gameObject);
    }

    public void PerformPause()
    {
        Debug.Log("Agent.PerformPause() called");
        
        if (pauseManager == null)
        {
            Debug.LogError("PauseManager is null! Trying to find it again...");
            pauseManager = FindObjectOfType<PauseManager>();
        }

        if (pauseManager != null)
        {
            Debug.Log("Calling pauseManager.TogglePause()");
            pauseManager.TogglePause();
        }
        else
        {
            Debug.LogError("PauseManager still null after retry!");
        }
    }

    private void AnimateCharacter()
    {
        Vector2 lookDirection = pointerInput - (Vector2)transform.position;
        agentAnimations.RotateToPointer(lookDirection);
        agentAnimations.PlayAnimation(MovementInput);
    }
}
