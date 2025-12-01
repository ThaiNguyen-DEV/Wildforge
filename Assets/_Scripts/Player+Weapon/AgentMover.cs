using UnityEngine;
using System.Collections;

public class AgentMover : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField]
    private float maxSpeed = 2, acceleration = 50, deceleration = 100;
    [SerializeField]
    private float currentSpeed = 0;
    private Vector2 oldMovementInput;
    public Vector2 MovementInput { get; set; }

    // Expose currentSpeed for animations
    public float CurrentSpeed => currentSpeed;

    // Forced velocity (dash, knockback, etc.)
    private bool overrideVelocity;
    private Vector2 forcedVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (overrideVelocity)
        {
            rb.linearVelocity = forcedVelocity;
            return;
        }

        if (MovementInput.magnitude > 0 && currentSpeed >= 0)
        {
            oldMovementInput = MovementInput.normalized;
            currentSpeed += acceleration * maxSpeed * Time.fixedDeltaTime;
        }
        else
        {
            currentSpeed -= deceleration * maxSpeed * Time.fixedDeltaTime;
        }

        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
        rb.linearVelocity = oldMovementInput * currentSpeed;
    }

    // Public API for dash / external forces
    public void SetForcedVelocity(Vector2 velocity, float duration)
    {
        StopCoroutineSafe(nameof(ForceVelocityRoutine));
        StartCoroutine(ForceVelocityRoutine(velocity, duration));
    }

    private IEnumerator ForceVelocityRoutine(Vector2 velocity, float duration)
    {
        overrideVelocity = true;
        forcedVelocity = velocity;
        currentSpeed = 0f;

        float end = Time.time + duration;
        // Hold forced velocity through physics ticks
        while (Time.time < end)
        {
            yield return new WaitForFixedUpdate();
        }

        overrideVelocity = false;
        forcedVelocity = Vector2.zero;
        currentSpeed = 0f; // stop on dash end
    }

    private void StopCoroutineSafe(string routine)
    {
        var c = GetComponent<MonoBehaviour>();
        // This class is itself a MonoBehaviour; safeguard to stop previous dash
        StopCoroutine(routine);
    }
}
