using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum AIType { Melee, Ranged }

    [Header("AI Settings")]
    [SerializeField]
    private AIType aiType = AIType.Melee;
    [SerializeField]
    private float detectionRadius = 10f;

    [Header("Ranged AI Settings")]
    [SerializeField]
    private float shootingRange = 7f;

    [Header("Melee AI Settings")]
    [SerializeField]
    private float meleeAttackRange = 1.5f;

    // Component references are now private and fetched automatically
    private WeaponBase weapon;
    private Transform playerTransform;
    private AgentMover agentMover;
    private AgentAnimations agentAnimations; // Added reference to animations
    private bool isPlayerInRange;

    private void Awake()
    {
        // Automatically get required components
        agentMover = GetComponent<AgentMover>();
        weapon = GetComponentInChildren<WeaponBase>();
        agentAnimations = GetComponentInChildren<AgentAnimations>(); // Get the animation component

        // Disable the Agent component if it exists, as it conflicts with AI control.
        if (TryGetComponent<Agent>(out var agent))
        {
            agent.enabled = false;
        }
    }

    public void SetPlayer(Transform player)
    {
        playerTransform = player;
    }

    private void Update()
    {
        // If player is not assigned, try to find it
        if (playerTransform == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                // If player still not found, do nothing
                agentMover.MovementInput = Vector2.zero;
                return;
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        isPlayerInRange = distanceToPlayer <= detectionRadius;

        Vector2 movementInput = Vector2.zero;

        if (isPlayerInRange)
        {
            Vector2 directionToPlayer = playerTransform.position - transform.position;

            // Point weapon towards player
            if (weapon != null)
            {
                weapon.PointerPosition = playerTransform.position;
            }

            // Animate character to face the player
            if (agentAnimations != null)
            {
                agentAnimations.RotateToPointer(directionToPlayer);
            }

            switch (aiType)
            {
                case AIType.Melee:
                    movementInput = HandleMeleeMovement();
                    break;
                case AIType.Ranged:
                    movementInput = HandleRangedMovement();
                    break;
            }
        }
        
        // Set the final movement input for the AgentMover
        agentMover.MovementInput = movementInput;

        // Play movement animation
        if (agentAnimations != null)
        {
            agentAnimations.PlayAnimation(movementInput);
        }
    }

    private Vector2 HandleMeleeMovement()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // If player is outside melee range, move closer
        if (distanceToPlayer > meleeAttackRange)
        {
            return (playerTransform.position - transform.position).normalized;
        }
        else // If player is inside melee range, stop and attack
        {
            weapon?.Attack();
            return Vector2.zero;
        }
    }

    private Vector2 HandleRangedMovement()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // If player is outside shooting range, move closer
        if (distanceToPlayer > shootingRange)
        {
            return (playerTransform.position - transform.position).normalized;
        }
        else // If player is inside shooting range, stop and shoot
        {
            weapon?.Attack();
            return Vector2.zero;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize detection and attack ranges
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (aiType == AIType.Ranged)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, shootingRange);
        }
        else if (aiType == AIType.Melee)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
        }
    }
}
