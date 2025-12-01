using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField]
    private float speed = 15f;
    [SerializeField]
    private int damage = 1;
    [SerializeField]
    private float lifetime = 3f;

    private Rigidbody2D rb;
    private GameObject sender;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Destroy the projectile after its lifetime expires to prevent clutter
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Initializes the projectile with a direction and the object that fired it.
    /// </summary>
    /// <param name="direction">The direction to travel in.</param>
    /// <param name="firedBy">The GameObject that fired the projectile.</param>
    public void Launch(Vector2 direction, GameObject firedBy)
    {
        sender = firedBy;
        rb.linearVelocity = direction.normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore collision with the object that fired the projectile
        if (other.gameObject == sender)
        {
            return;
        }

        // Check if the collided object has a Health component
        if (other.TryGetComponent<Health>(out Health health))
        {
            health.GetHit(damage, sender);
        }

        // Destroy the projectile on impact with any solid object
        // You might want to expand this logic (e.g., ignore other triggers)
        Destroy(gameObject);
    }
}