using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Base Weapon Settings")]
    public SpriteRenderer characterRenderer;
    public SpriteRenderer weaponRenderer;
    public float delay = 0.3f;

    protected bool attackBlocked;
    public Vector2 PointerPosition { get; set; }
    public bool IsAttacking { get; protected set; }

    protected virtual void Update()
    {
        // This check is what was causing the issue.
        // We want the weapon to aim even if it's in an attack cooldown.
        // if (IsAttacking)
        //     return;

        // Point the weapon towards the cursor/player
        Vector2 direction = (PointerPosition - (Vector2)transform.position).normalized;
        transform.right = direction;

        // Flip the weapon sprite based on direction
        Vector2 scale = transform.localScale;
        scale.y = direction.x < 0 ? -1 : 1;
        transform.localScale = scale;

        // Adjust sorting order to render correctly in front of or behind the player
        if (transform.eulerAngles.z > 0 && transform.eulerAngles.z < 180)
        {
            weaponRenderer.sortingOrder = characterRenderer.sortingOrder - 1;
        }
        else
        {
            weaponRenderer.sortingOrder = characterRenderer.sortingOrder + 1;
        }
    }

    // Abstract methods that all weapons must implement
    public abstract void Attack();
    public virtual void ResetAttack()
    {
        IsAttacking = false;
    }
}